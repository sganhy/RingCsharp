using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Extensions;
using Ring.PostgreSQL.Models;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Ring.PostgreSQL.Helpers;

internal static class AuthenticationHelper
{
	private const string Gs2Header = "n,,";
	private const int MaxScramIterations = 10_000_000;

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
						throw ParseErrorFromSequence(msg.Body).ToPgOperationalError();
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

				default:
					// FIX: any other message used to fall through and be parsed as an auth request.
					var unexpected = UnexpectedMessage(msg.Code, "AuthenticationRequest");
					reader.AdvanceTo(msg.EndPosition);
					throw unexpected;
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
					// FIX: Span<byte> locals are not allowed in async methods; use a byte[].
					var salt = msg.Body.Slice(4, 4).ToArray();
					reader.AdvanceTo(msg.EndPosition);
					await writer.SendPasswordMessageAsync(ComputeMD5Password(user, password, salt), cancellationToken).ConfigureAwait(false);
					continue;

				case AuthenticationType.SASL:
					var mechanisms = msg.Body.Slice(4).ToArray(); // materialized so it can cross awaits
					reader.AdvanceTo(msg.EndPosition);
					await AuthenticateSASLAsync(reader, writer, mechanisms, password, cancellationToken).ConfigureAwait(false);
					// FIX: the server still sends AuthenticationOk after SASLFinal; let the loop consume it.
					continue;

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

	// Only does I/O. All Span/stackalloc work lives in synchronous helpers below,
	// because ref-struct locals cannot live across an await.
	private static async Task AuthenticateSASLAsync(
		PipeReader reader,
		PipeWriter writer,
		byte[] mechanismsPayload,
		string password,
		CancellationToken cancellationToken = default)
	{
		if (!ContainsMechanism(mechanismsPayload, "SCRAM-SHA-256"u8))
			throw new NotSupportedException("Server does not offer SCRAM-SHA-256; no other SASL mechanism is implemented.");

		var clientNonce = GetNonce();
		var clientFirstBare = $"n=*,r={clientNonce}";

		await writer.SendSASLInitialResponseAsync("SCRAM-SHA-256", Encoding.UTF8.GetBytes(Gs2Header + clientFirstBare), cancellationToken).ConfigureAwait(false);

		var msg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
		string serverFirstMessage;
		try
		{
			ThrowIfError(msg);
			if (msg.Code != (byte)BackendMessageCode.AuthenticationRequest || GetAuthenticationType(msg.Body, 0) != AuthenticationType.SASLContinue)
				throw UnexpectedMessage(msg.Code, "AuthenticationSASLContinue");

			serverFirstMessage = Encoding.UTF8.GetString(msg.Body.Slice(4));
		}
		finally
		{
			reader.AdvanceTo(msg.EndPosition);
		}

		var (clientFinalMessage, expectedServerSignature) = ComputeScramClientFinal(password, clientNonce, clientFirstBare, serverFirstMessage);

		await writer.SendSASLResponseAsync(Encoding.UTF8.GetBytes(clientFinalMessage), cancellationToken).ConfigureAwait(false);

		msg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
		string serverFinalMessage;
		try
		{
			ThrowIfError(msg);
			if (msg.Code != (byte)BackendMessageCode.AuthenticationRequest || GetAuthenticationType(msg.Body, 0) != AuthenticationType.SASLFinal)
				throw UnexpectedMessage(msg.Code, "AuthenticationSASLFinal");

			serverFinalMessage = Encoding.UTF8.GetString(msg.Body.Slice(4));
		}
		finally
		{
			reader.AdvanceTo(msg.EndPosition);
		}

		VerifyServerFinal(serverFinalMessage, expectedServerSignature);
	}

	private static (string ClientFinalMessage, byte[] ExpectedServerSignature) ComputeScramClientFinal(
		string password,
		string clientNonce,
		string clientFirstBare,
		string serverFirstMessage)
	{
		var (serverNonce, salt, iterations) = ParseServerFirstMessage(serverFirstMessage.AsSpan());
		if (!serverNonce.StartsWith(clientNonce, StringComparison.Ordinal))
			throw new InvalidOperationException("SCRAM: server nonce does not start with the client nonce.");
		if (iterations <= 0 || iterations > MaxScramIterations)
			throw new InvalidOperationException($"SCRAM: unreasonable iteration count {iterations}.");

		var clientFinalNoProof = $"c={Convert.ToBase64String(Encoding.UTF8.GetBytes(Gs2Header))},r={serverNonce}";
		var authMessage = $"{clientFirstBare},{serverFirstMessage},{clientFinalNoProof}";

		Span<byte> saltedPassword = stackalloc byte[32];
		Span<byte> clientKey = stackalloc byte[32];
		var authMessageByteCount = Encoding.UTF8.GetByteCount(authMessage);
		byte[]? rentedAuthMsg = null;
		Span<byte> authMessageBytes = authMessageByteCount <= 512
			? stackalloc byte[authMessageByteCount]
			: (rentedAuthMsg = ArrayPool<byte>.Shared.Rent(authMessageByteCount)).AsSpan(0, authMessageByteCount);

		try
		{
			Rfc2898DeriveBytes.Pbkdf2(password, salt, saltedPassword, iterations, HashAlgorithmName.SHA256);
			HMACSHA256.HashData(saltedPassword, "Client Key"u8, clientKey);

			Span<byte> storedKey = stackalloc byte[32];
			SHA256.HashData(clientKey, storedKey);

			Encoding.UTF8.GetBytes(authMessage, authMessageBytes);

			Span<byte> clientSignature = stackalloc byte[32];
			HMACSHA256.HashData(storedKey, authMessageBytes, clientSignature);

			Span<byte> clientProof = stackalloc byte[32];
			XorSpans(clientKey, clientSignature, clientProof);

			Span<byte> serverKey = stackalloc byte[32];
			HMACSHA256.HashData(saltedPassword, "Server Key"u8, serverKey);

			Span<byte> serverSignature = stackalloc byte[32];
			HMACSHA256.HashData(serverKey, authMessageBytes, serverSignature);

			var clientFinalMessage = $"{clientFinalNoProof},p={Convert.ToBase64String(clientProof)}";
			return (clientFinalMessage, serverSignature.ToArray());
		}
		finally
		{
			CryptographicOperations.ZeroMemory(saltedPassword);
			CryptographicOperations.ZeroMemory(clientKey);
			if (rentedAuthMsg is not null) ArrayPool<byte>.Shared.Return(rentedAuthMsg);
		}
	}

	private static void VerifyServerFinal(string serverFinalMessage, byte[] expectedSignature)
	{
		if (serverFinalMessage.StartsWith("e=", StringComparison.Ordinal))
			throw new InvalidOperationException($"SCRAM: server reported an error: {serverFinalMessage[2..]}");

		if (!serverFinalMessage.StartsWith("v=", StringComparison.Ordinal))
			throw new InvalidOperationException("SCRAM: malformed server-final-message.");

		var value = serverFinalMessage.AsSpan(2);
		var comma = value.IndexOf(',');
		if (comma >= 0) value = value[..comma];

		Span<byte> received = stackalloc byte[32];
		// FIX: compare decoded bytes in constant time instead of a string StartsWith.
		if (!Convert.TryFromBase64Chars(value, received, out var written)
			|| written != expectedSignature.Length
			|| !CryptographicOperations.FixedTimeEquals(received[..written], expectedSignature))
		{
			throw new InvalidOperationException("SCRAM: server signature verification failed - possible spoofed server.");
		}
	}

	private static bool ContainsMechanism(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> target)
	{
		var offset = 0;
		while (offset < payload.Length && payload[offset] != 0)
		{
			var slice = payload.Slice(offset);
			var nullIdx = slice.IndexOf((byte)0);
			if (nullIdx < 0) nullIdx = slice.Length;

			var mech = slice.Slice(0, nullIdx);
			if (mech.SequenceEqual(target)) return true;

			offset += nullIdx + 1;
		}
		return false;
	}

	private static AuthenticationType GetAuthenticationType(ReadOnlySequence<byte> body, int offset)
	{
		Span<byte> temp = stackalloc byte[4];
		body.Slice(offset, 4).CopyTo(temp);
		return BinaryPrimitives.ReadInt32BigEndian(temp).ToAuthenticationType();
	}

	[SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "MD5 is required by the PostgreSQL wire protocol for legacy md5 authentication.")]
	private static string ComputeMD5Password(string username, string password, ReadOnlySpan<byte> salt)
	{
		// md5( md5(password + username) as lowercase hex + salt ), prefixed with "md5"
		Span<byte> innerInput = stackalloc byte[Encoding.UTF8.GetByteCount(password) + Encoding.UTF8.GetByteCount(username)];
		var innerWritten = Encoding.UTF8.GetBytes(password, innerInput);
		Encoding.UTF8.GetBytes(username, innerInput[innerWritten..]);

		Span<byte> innerHash = stackalloc byte[16];
		MD5.HashData(innerInput, innerHash);

		// FIX: the inner hex must be LOWERCASE (it was being uppercased, which breaks md5 auth).
		Span<char> innerHex = stackalloc char[32];
		ToHexLower(innerHash, innerHex);

		Span<byte> outerInput = stackalloc byte[32 + salt.Length];
		Encoding.UTF8.GetBytes(innerHex, outerInput);
		salt.CopyTo(outerInput[32..]);

		Span<byte> outerHash = stackalloc byte[16];
		MD5.HashData(outerInput, outerHash);

		Span<char> outerHex = stackalloc char[32];
		ToHexLower(outerHash, outerHex);

		return "md5" + outerHex.ToString();
	}

	// Lowercase hex without relying on newer Convert APIs (TryToHexStringLower is .NET 9+).
	private static void ToHexLower(ReadOnlySpan<byte> bytes, Span<char> chars)
	{
		const string hex = "0123456789abcdef";
		for (var i = 0; i < bytes.Length; i++)
		{
			chars[i * 2] = hex[bytes[i] >> 4];
			chars[i * 2 + 1] = hex[bytes[i] & 0xF];
		}
	}

	private static string GetNonce()
	{
		Span<byte> bytes = stackalloc byte[18];
		RandomNumberGenerator.Fill(bytes);
		return Convert.ToBase64String(bytes);
	}

	private static void ThrowIfError(in MessageSlice msg)
	{
		if (msg.Code == (byte)BackendMessageCode.ErrorResponse)
			throw ParseErrorFromSequence(msg.Body).ToPgOperationalError();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Ring.Data.OperationalError ParseErrorFromSequence(ReadOnlySequence<byte> sequence)
	{
		if (sequence.IsSingleSegment)
		{
			return sequence.FirstSpan.ParseErrorFields();
		}

		var length = (int)sequence.Length;
		byte[]? rented = null;
		Span<byte> buffer = length <= 128 ? stackalloc byte[length] : (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length);

		try
		{
			sequence.CopyTo(buffer);
			// FIX: extension methods don't apply the Span -> ReadOnlySpan user-defined conversion; cast explicitly.
			return ((ReadOnlySpan<byte>)buffer).ParseErrorFields();
		}
		finally
		{
			if (rented != null)
				ArrayPool<byte>.Shared.Return(rented);
		}
	}

	private static (string Nonce, byte[] Salt, int Iterations) ParseServerFirstMessage(ReadOnlySpan<char> message)
	{
		ReadOnlySpan<char> nonce = default;
		ReadOnlySpan<char> saltBase64 = default;
		ReadOnlySpan<char> iterations = default;

		while (!message.IsEmpty)
		{
			var idx = message.IndexOf(',');
			var part = idx >= 0 ? message.Slice(0, idx) : message;

			if (part.StartsWith("r=")) nonce = part[2..];
			else if (part.StartsWith("s=")) saltBase64 = part[2..];
			else if (part.StartsWith("i=")) iterations = part[2..];

			message = idx >= 0 ? message[(idx + 1)..] : default;
		}

		if (nonce.IsEmpty || saltBase64.IsEmpty || iterations.IsEmpty)
			throw new InvalidOperationException("SCRAM: malformed server-first-message.");

		return (
			nonce.ToString(),
			Convert.FromBase64String(saltBase64.ToString()),
			int.Parse(iterations, NumberStyles.None, CultureInfo.InvariantCulture)
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void XorSpans(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, Span<byte> destination)
	{
		for (var i = 0; i < a.Length; i++)
			destination[i] = (byte)(a[i] ^ b[i]);
	}

	private static InvalidOperationException UnexpectedMessage(byte code, string expected) =>
		new($"Unexpected message '{(char)code}' from server; expected {expected}.");
}
