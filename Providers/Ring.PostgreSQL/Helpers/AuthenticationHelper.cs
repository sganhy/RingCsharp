using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Extensions;

namespace Ring.PostgreSQL.Helpers;

internal static class AuthenticationHelper
{
	internal static async Task<(int? BackendPid, int? BackendSecret)> HandleAuthenticationAsync(
		PipeReader reader,
		PipeWriter writer,
		string user,
		string password,
		CancellationToken cancellationToken = default)
	{
		while (true)
		{
			var msg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);

			switch ((BackendMessageCode)msg.Code)
			{
				case BackendMessageCode.ErrorResponse:
					try
					{
						throw msg.ToByteArray().ParseErrorFields().ToPgOperationalError();
					}
					finally
					{
						reader.AdvanceTo(msg.EndPosition);
					}

				case BackendMessageCode.NoticeResponse:
					reader.AdvanceTo(msg.EndPosition);
					continue;

				case BackendMessageCode.AuthenticationRequest:
					break;
			}

			var authType = GetAuthenticationType(msg.Body, 0);
			switch (authType)
			{
				case AuthenticationType.Ok:
					reader.AdvanceTo(msg.EndPosition);
					return await reader.WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);

				case AuthenticationType.CleartextPassword:
					reader.AdvanceTo(msg.EndPosition);
					await writer.SendPasswordMessageAsync(password, cancellationToken).ConfigureAwait(false);
					continue;

				case AuthenticationType.MD5Password:
					var salt = msg.Body.Slice(4, 4).ToArray();
					reader.AdvanceTo(msg.EndPosition);
					await writer.SendPasswordMessageAsync(ComputeMD5Password(user, password, salt), cancellationToken).ConfigureAwait(false);
					continue;

				case AuthenticationType.SASL:
					var mechanismsPayload = msg.Body.Slice(4).ToArray();
					reader.AdvanceTo(msg.EndPosition);
					return await AuthenticateSASLAsync(reader, writer, mechanismsPayload, password, cancellationToken).ConfigureAwait(false);

				case AuthenticationType.GSS:
				case AuthenticationType.SSPI:
					reader.AdvanceTo(msg.EndPosition);
					throw new NotSupportedException("GSSAPI/SSPI authentication is not implemented by this driver.");

				default:
					reader.AdvanceTo(msg.EndPosition);
					throw new NotSupportedException($"Authentication method {authType} is not supported by this driver.");
			}
		}
	}

	private static async Task<(int? BackendPid, int? BackendSecret)> AuthenticateSASLAsync(
		PipeReader reader,
		PipeWriter writer,
		byte[] mechanismsPayload,
		string password,
		CancellationToken cancellationToken = default)
	{
		var mechanisms = ParseNullTerminatedList(mechanismsPayload);
		if (!mechanisms.Contains("SCRAM-SHA-256"))
			throw new NotSupportedException("Server does not offer SCRAM-SHA-256; no other SASL mechanism is implemented.");

		const string gs2Header = "n,,";
		var clientNonce = GetNonce();
		var clientFirstBare = $"n=*,r={clientNonce}";

		await writer.SendSASLInitialResponseAsync("SCRAM-SHA-256", Encoding.UTF8.GetBytes(gs2Header + clientFirstBare), cancellationToken).ConfigureAwait(false);

		var msg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
		string serverFirstMessage;
		try
		{
			ThrowIfError(msg);
			if (msg.Code != (byte)BackendMessageCode.AuthenticationRequest || GetAuthenticationType(msg.Body, 0) != AuthenticationType.SASLContinue)
				throw UnexpectedMessage(msg.Code, "AuthenticationSASLContinue");

			serverFirstMessage = Encoding.UTF8.GetString(msg.Body.Slice(4).ToArray());
		}
		finally
		{
			reader.AdvanceTo(msg.EndPosition);
		}

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
		await writer.SendSASLResponseAsync(Encoding.UTF8.GetBytes(clientFinalMessage), cancellationToken).ConfigureAwait(false);

		msg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
		string serverFinalMessage;
		try
		{
			ThrowIfError(msg);
			if (msg.Code != (byte)BackendMessageCode.AuthenticationRequest || GetAuthenticationType(msg.Body, 0) != AuthenticationType.SASLFinal)
				throw UnexpectedMessage(msg.Code, "AuthenticationSASLFinal");

			serverFinalMessage = Encoding.UTF8.GetString(msg.Body.Slice(4).ToArray());
		}
		finally
		{
			reader.AdvanceTo(msg.EndPosition);
		}

		var expectedSignature = Convert.ToBase64String(HmacSha256(HmacSha256(saltedPassword, "Server Key"), authMessage));
		if (!serverFinalMessage.StartsWith($"v={expectedSignature}", StringComparison.Ordinal))
			throw new InvalidOperationException("SCRAM: server signature verification failed - possible spoofed server.");

		return await reader.WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);
	}

	private static AuthenticationType GetAuthenticationType(ReadOnlySequence<byte> body, int offset)
	{
		Span<byte> temp = stackalloc byte[4];
		body.Slice(offset, 4).CopyTo(temp);
		return BinaryPrimitives.ReadInt32BigEndian(temp).ToAuthenticationType();
	}

	[SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "MD5 is required by the PostgreSQL wire protocol for legacy md5 authentication.")]
	private static string ComputeMD5Password(string username, string password, byte[] salt)
	{
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
		var inner = MD5.HashData(Encoding.UTF8.GetBytes(password + username));
		var innerHex = Convert.ToHexString(inner).ToUpperInvariant();

		var outerInput = new byte[innerHex.Length + salt.Length];
		Encoding.UTF8.GetBytes(innerHex, outerInput);
		salt.CopyTo(outerInput, innerHex.Length);

		return "md5" + Convert.ToHexString(MD5.HashData(outerInput)).ToLowerInvariant();
#pragma warning restore CA5351
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

	private static void ThrowIfError(in PipeReaderExtensions.MessageSlice msg)
	{
		if (msg.Code == (byte)BackendMessageCode.ErrorResponse)
			throw msg.ToByteArray().ParseErrorFields().ToPgOperationalError();
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