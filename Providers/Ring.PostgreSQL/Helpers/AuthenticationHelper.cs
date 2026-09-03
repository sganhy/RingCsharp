using Ring.Data;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Exceptions;
using Ring.PostgreSQL.Extensions;
using System.Buffers.Binary;
using System.Globalization;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Ring.PostgreSQL.Helpers;

internal static class AuthenticationHelper
{
	internal static async Task<(int? BackendPid, int? BackendSecret)> HandleAuthenticationAsync(NetworkStream stream, string user, string password, CancellationToken cancellationToken = default)
	{
		while (true)
		{
			var (code, body) = await stream.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);

			switch ((BackendMessageCode)code)
			{
				case BackendMessageCode.ErrorResponse:
					throw body.ParseErrorFields().ToPgOperationalError();
				case BackendMessageCode.NoticeResponse:
					continue;
				case BackendMessageCode.AuthenticationRequest:
					break;
				default:
					//throw UnexpectedMessage(code, "an authentication message");
					break;
			}

			var authType = GetAuthenticationType(body, 0);
			switch (authType)
			{
				case AuthenticationType.Ok:
					return await stream.WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);
				case AuthenticationType.CleartextPassword:
					await stream.SendPasswordMessageAsync(password, cancellationToken).ConfigureAwait(false);
					continue;
				case AuthenticationType.MD5Password:
					await stream.SendPasswordMessageAsync(ComputeMD5Password(user, password, body[4..8]), cancellationToken).ConfigureAwait(false);
					continue;
				case AuthenticationType.SASL:
					return await AuthenticateSASLAsync(stream, body[4..], password, cancellationToken).ConfigureAwait(false);
				case AuthenticationType.GSS:
				case AuthenticationType.SSPI:
					throw new NotSupportedException("GSSAPI/SSPI authentication is not implemented by this driver.");

				default:
					throw new NotSupportedException($"Authentication method {authType} is not supported by this driver.");
			}
		}
	}

	private static async Task<(int? BackendPid, int? BackendSecret)> AuthenticateSASLAsync(NetworkStream stream, byte[] mechanismsPayload, string password, CancellationToken cancellationToken = default)
	{
		var mechanisms = ParseNullTerminatedList(mechanismsPayload);
		if (!mechanisms.Contains("SCRAM-SHA-256"))
			throw new NotSupportedException("Server does not offer SCRAM-SHA-256; no other SASL mechanism is implemented.");

		const string gs2Header = "n,,"; // "n" = client does not support channel binding
		var clientNonce = GetNonce();
		var clientFirstBare = $"n=*,r={clientNonce}";

		await stream.SendSASLInitialResponseAsync("SCRAM-SHA-256", Encoding.UTF8.GetBytes(gs2Header + clientFirstBare), cancellationToken).ConfigureAwait(false);

		var (code, body) = await stream.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
		ThrowIfError(code, body);
		if (code != (byte)BackendMessageCode.AuthenticationRequest || ReadInt32BE(body, 0) != (int)AuthenticationType.SASLContinue)
			throw UnexpectedMessage(code, "AuthenticationSASLContinue");

		var serverFirstMessage = Encoding.UTF8.GetString(body, 4, body.Length - 4);
		var (serverNonce, salt, iterations) = ParseServerFirstMessage(serverFirstMessage);
		if (!serverNonce.StartsWith(clientNonce, StringComparison.Ordinal))
			throw new InvalidOperationException("SCRAM: server nonce does not start with the client nonce.");

		var clientFinalNoProof = $"c={Convert.ToBase64String(Encoding.UTF8.GetBytes(gs2Header))},r={serverNonce}";
		var authMessage = $"{clientFirstBare},{serverFirstMessage},{clientFinalNoProof}";

		var saltedPassword = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 256 / 8);
		var clientKey = HmacSha256(saltedPassword, "Client Key");
		var storedKey = SHA256.HashData(clientKey);
		var clientProof = Xor(clientKey, HmacSha256(storedKey, authMessage));

		var clientFinalMessage = $"{clientFinalNoProof},p={Convert.ToBase64String(clientProof)}";
		await stream.SendSASLResponseAsync(Encoding.UTF8.GetBytes(clientFinalMessage), cancellationToken).ConfigureAwait(false);

		(code, body) = await stream.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
		ThrowIfError(code, body);
		if (code != (byte)BackendMessageCode.AuthenticationRequest || ReadInt32BE(body, 0) != (int)AuthenticationType.SASLFinal)
			throw UnexpectedMessage(code, "AuthenticationSASLFinal");

		var serverFinalMessage = Encoding.UTF8.GetString(body, 4, body.Length - 4);
		var expectedSignature = Convert.ToBase64String(HmacSha256(HmacSha256(saltedPassword, "Server Key"), authMessage));
		if (!serverFinalMessage.StartsWith($"v={expectedSignature}", StringComparison.Ordinal))
			throw new InvalidOperationException("SCRAM: server signature verification failed - possible spoofed server.");

		return await stream.WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);
	}

	private static int ReadInt32BE(byte[] data, int offset) => BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offset));
	private static AuthenticationType GetAuthenticationType(byte[] data, int offset) => ReadInt32BE(data, offset).ToAuthenticationType();

	private static string ComputeMD5Password(string username, string password, byte[] salt)
	{
		var inner = MD5.HashData(Encoding.UTF8.GetBytes(password + username));
		var innerHex = Convert.ToHexString(inner).ToLowerInvariant();

		var outerInput = new byte[innerHex.Length + salt.Length];
		Encoding.UTF8.GetBytes(innerHex, outerInput);
		salt.CopyTo(outerInput, innerHex.Length);

		return "md5" + Convert.ToHexString(MD5.HashData(outerInput)).ToLowerInvariant();
	}

	private static List<string> ParseNullTerminatedList(byte[] body)
	{
		var values = new List<string>();
		var offset = 0;
		while (offset < body.Length && body[offset] != 0)
			values.Add(ArrayExtensions.ReadCString(body, ref offset));
		return values;
	}

	private static string GetNonce()
	{
		var bytes = new byte[18];
		RandomNumberGenerator.Fill(bytes);
		return Convert.ToBase64String(bytes);
	}

	private static void ThrowIfError(byte code, byte[] body)
	{
		if (code == (byte)BackendMessageCode.ErrorResponse)
			throw body.ParseErrorFields().ToPgOperationalError();
	}

	private static (string Nonce, byte[] Salt, int Iterations) ParseServerFirstMessage(string message)
	{
		string? nonce = null, saltBase64 = null, iterations = null;
		foreach (var part in message.Split(','))
		{
			if (part.StartsWith("r=", StringComparison.Ordinal)) nonce = part[2..];
			else if (part.StartsWith("s=", StringComparison.Ordinal)) saltBase64 = part[2..];
			else if (part.StartsWith("i=", StringComparison.Ordinal)) iterations = part[2..];
		}
		if (nonce is null || saltBase64 is null || iterations is null)
			throw new InvalidOperationException($"SCRAM: malformed server-first-message '{message}'.");

		return (nonce, Convert.FromBase64String(saltBase64), int.Parse(iterations, CultureInfo.InvariantCulture));
	}

	private static byte[] Xor(byte[] a, byte[] b)
	{
		var result = new byte[a.Length];
		for (var i = 0; i < a.Length; i++) result[i] = (byte)(a[i] ^ b[i]);
		return result;
	}

	private static byte[] HmacSha256(byte[] key, string data) => HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(data));

	private static InvalidOperationException UnexpectedMessage(byte code, string expected) =>
		new($"Unexpected message '{(char)code}' from server; expected {expected}.");
}
