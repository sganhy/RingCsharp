using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Ring.PostgreSQL.Helpers;

/// <summary>
/// Client side of SCRAM-SHA-256 (RFC 5802 / RFC 7677) as used by PostgreSQL's SASL authentication.
/// One instance per handshake: CreateClientFirstMessage -> ProcessServerFirst -> VerifyServerFinal.
/// Channel binding (SCRAM-SHA-256-PLUS) is not supported: gs2 header is always "n,,".
/// </summary>
internal sealed class ScramSha256Client : IDisposable
{
	internal const string MechanismName = "SCRAM-SHA-256";

	private const string Gs2Header = "n,,";
	private const string ChannelBindingBase64 = "biws"; // base64("n,,")
	private const int MaxIterations = 1_000_000;        // protects against a hostile server asking for hours of PBKDF2

	private readonly string _password;
	private string? _clientNonce;
	private string? _clientFirstBare;
	private byte[]? _serverSignature;

	/// <summary>True once the server signature (v=...) has been verified. Authentication must not be accepted before.</summary>
	public bool IsServerVerified { get; private set; }

	internal ScramSha256Client(string password) => _password = password;

	/// <summary>Builds "n,,n=,r=&lt;nonce&gt;". PostgreSQL ignores the SCRAM user name and uses the one from the startup message.</summary>
	internal byte[] CreateClientFirstMessage()
	{
		Span<byte> nonceBytes = stackalloc byte[18];
		RandomNumberGenerator.Fill(nonceBytes);

		_clientNonce = Convert.ToBase64String(nonceBytes); // 24 chars, no ',' possible
		_clientFirstBare = "n=,r=" + _clientNonce;
		return Encoding.UTF8.GetBytes(Gs2Header + _clientFirstBare);
	}

	/// <summary>Consumes the server-first message (r=,s=,i=) and returns the client-final message (c=,r=,p=).</summary>
	internal byte[] ProcessServerFirst(ReadOnlySpan<byte> serverFirstBytes)
	{
		if (_clientNonce is null || _clientFirstBare is null)
			throw Violation("SCRAM server-first received before client-first was sent.");

		var serverFirst = Encoding.UTF8.GetString(serverFirstBytes);

		string? nonce = null, salt = null;
		var iterations = 0;

		foreach (var part in serverFirst.Split(','))
		{
			if (part.Length < 2 || part[1] != '=') continue;
			switch (part[0])
			{
				case 'r': nonce = part[2..]; break;
				case 's': salt = part[2..]; break;
				case 'i': int.TryParse(part.AsSpan(2), NumberStyles.None, CultureInfo.InvariantCulture, out iterations); break;
			}
		}

		if (nonce is null || salt is null)
			throw Violation("SCRAM server-first message is missing the nonce or the salt.");
		if (!nonce.StartsWith(_clientNonce, StringComparison.Ordinal) || nonce.Length == _clientNonce.Length)
			throw Violation("SCRAM server nonce does not extend the client nonce.");
		if (iterations < 1 || iterations > MaxIterations)
			throw Violation($"SCRAM iteration count {iterations} is invalid or too large.");

		byte[] saltBytes;
		try { saltBytes = Convert.FromBase64String(salt); }
		catch (FormatException) { throw Violation("SCRAM salt is not valid base64."); }

		var passwordBytes = Encoding.UTF8.GetBytes(Normalize(_password));
		byte[]? saltedPassword = null, clientKey = null, storedKey = null, clientSignature = null, serverKey = null;
		try
		{
			saltedPassword = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, iterations, HashAlgorithmName.SHA256, 32);

			clientKey = HMACSHA256.HashData(saltedPassword, "Client Key"u8);
			storedKey = SHA256.HashData(clientKey);

			var clientFinalWithoutProof = $"c={ChannelBindingBase64},r={nonce}";
			var authMessage = Encoding.UTF8.GetBytes($"{_clientFirstBare},{serverFirst},{clientFinalWithoutProof}");

			clientSignature = HMACSHA256.HashData(storedKey, authMessage);

			var proof = new byte[clientKey.Length];
			for (var i = 0; i < proof.Length; i++)
				proof[i] = (byte)(clientKey[i] ^ clientSignature[i]);

			serverKey = HMACSHA256.HashData(saltedPassword, "Server Key"u8);
			_serverSignature = HMACSHA256.HashData(serverKey, authMessage);

			return Encoding.UTF8.GetBytes($"{clientFinalWithoutProof},p={Convert.ToBase64String(proof)}");
		}
		finally
		{
			CryptographicOperations.ZeroMemory(passwordBytes);
			Zero(saltedPassword); Zero(clientKey); Zero(storedKey); Zero(clientSignature); Zero(serverKey);
		}
	}

	/// <summary>Verifies the server-final message ("v=&lt;signature&gt;"), proving the server knows our verifier.</summary>
	internal void VerifyServerFinal(ReadOnlySpan<byte> serverFinalBytes)
	{
		if (_serverSignature is null)
			throw Violation("SCRAM server-final received before the exchange completed.");

		var text = Encoding.UTF8.GetString(serverFinalBytes);
		if (text.StartsWith("e=", StringComparison.Ordinal))
			throw Violation($"SCRAM authentication failed: {text[2..]}");
		if (!text.StartsWith("v=", StringComparison.Ordinal))
			throw Violation("SCRAM server-final message has no verifier.");

		var end = text.IndexOf(',');
		var b64 = end < 0 ? text[2..] : text[2..end];

		byte[] received;
		try { received = Convert.FromBase64String(b64); }
		catch (FormatException) { throw Violation("SCRAM server signature is not valid base64."); }

		if (!CryptographicOperations.FixedTimeEquals(received, _serverSignature))
			throw Violation("SCRAM server signature mismatch: the server could not be verified.");

		IsServerVerified = true;
	}

	public void Dispose() => Zero(_serverSignature);

	#region private methods

	// Approximation of SASLprep (RFC 4013): NFKC. Like PostgreSQL, fall back to the raw password if it can't be normalized.
	private static string Normalize(string password)
	{
		try { return password.Normalize(NormalizationForm.FormKC); }
		catch (ArgumentException) { return password; }
	}

	private static void Zero(byte[]? data)
	{
		if (data is not null) CryptographicOperations.ZeroMemory(data);
	}

	private static InvalidOperationException Violation(string message) => new(message);

	#endregion
}
