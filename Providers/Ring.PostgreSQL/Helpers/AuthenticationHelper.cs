using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Exceptions;
using Ring.PostgreSQL.Extensions;

namespace Ring.PostgreSQL.Helpers;

internal static class AuthenticationHelper
{
	internal static async ValueTask<(int? BackendPid, int? BackendSecret)> HandleAuthenticationAsync(
		PipeReader reader,
		PipeWriter writer,
		string username,
		string password,
		CancellationToken cancellationToken = default)
	{
		int? backendPid = null;
		int? backendSecret = null;

		while (true)
		{
			var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
			var buffer = result.Buffer;

			if (buffer.IsEmpty && result.IsCompleted)
			{
				ThrowUnexpectedEof();
			}

			if (!TryProcessAuthMessage(buffer, out var messageLength))
			{
				reader.AdvanceTo(buffer.Start, buffer.End);
				continue;
			}

			var (consumed, isComplete, pid, secret) = await ProcessAuthMessageDetailsAsync(buffer, messageLength, writer, username, password, cancellationToken).ConfigureAwait(false);

			if (pid.HasValue) backendPid = pid;
			if (secret.HasValue) backendSecret = secret;

			reader.AdvanceTo(consumed);

			if (isComplete)
			{
				return (backendPid, backendSecret);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool TryProcessAuthMessage(ReadOnlySequence<byte> buffer, out int messageLength)
	{
		if (buffer.Length < 5)
		{
			messageLength = 0;
			return false;
		}

		Span<byte> header = stackalloc byte[5];
		buffer.Slice(0, 5).CopyTo(header);

		messageLength = BinaryPrimitives.ReadInt32BigEndian(header.Slice(1, 4));
		return buffer.Length >= messageLength + 1;
	}

	private static async ValueTask<(SequencePosition Consumed, bool IsComplete, int? Pid, int? Secret)> ProcessAuthMessageDetailsAsync(
		ReadOnlySequence<byte> buffer,
		int messageLength,
		PipeWriter writer,
		string username,
		string password,
		CancellationToken cancellationToken)
	{
		Span<byte> header = stackalloc byte[5];
		buffer.Slice(0, 5).CopyTo(header);

		var messageType = (BackendMessageCode)header[0];
		var messagePayload = buffer.Slice(5, messageLength - 4);
		var isComplete = false;
		int? pid = null;
		int? secret = null;

		switch (messageType)
		{
			case BackendMessageCode.AuthenticationRequest:
				await ProcessAuthenticationRequestAsync(messagePayload, writer, username, password, cancellationToken).ConfigureAwait(false);
				break;

			case BackendMessageCode.BackendKeyData:
				ProcessBackendKeyData(messagePayload, out pid, out secret);
				break;

			case BackendMessageCode.ReadyForQuery:
				isComplete = true;
				break;

			case BackendMessageCode.ErrorResponse:
				var error = messagePayload.IsSingleSegment
					? messagePayload.FirstSpan.ParseErrorFields()
					: messagePayload.ToArray().AsSpan().ParseErrorFields();
				throw new PgOperationalError(error.Message);

			default:
				break;
		}

		var nextPosition = buffer.GetPosition(messageLength + 1);
		return (nextPosition, isComplete, pid, secret);
	}

	private static async ValueTask ProcessAuthenticationRequestAsync(
		ReadOnlySequence<byte> payload,
		PipeWriter writer,
		string username,
		string password,
		CancellationToken cancellationToken)
	{
		Span<byte> authTypeBuffer = stackalloc byte[4];
		payload.Slice(0, 4).CopyTo(authTypeBuffer);
		var authType = (AuthenticationType)BinaryPrimitives.ReadInt32BigEndian(authTypeBuffer);

		switch (authType)
		{
			case AuthenticationType.Ok:
				break;

			case AuthenticationType.CleartextPassword:
				await SendCleartextPasswordAsync(writer, password, cancellationToken).ConfigureAwait(false);
				break;

			case AuthenticationType.MD5Password:
				byte[] saltArray = ArrayPool<byte>.Shared.Rent(4);
				try
				{
					payload.Slice(4, 4).CopyTo(saltArray);
					await SendMd5PasswordAsync(writer, username, password, saltArray.AsMemory(0, 4), cancellationToken).ConfigureAwait(false);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(saltArray);
				}
				break;

			case AuthenticationType.GSS:
			case AuthenticationType.GSSContinue:
			case AuthenticationType.SSPI:
			case AuthenticationType.SASL:
			case AuthenticationType.SASLContinue:
			case AuthenticationType.SASLFinal:
				ThrowSaslNotImplemented();
				break;

			default:
				ThrowUnsupportedAuthType(authType);
				break;
		}
	}

	private static async ValueTask SendCleartextPasswordAsync(PipeWriter writer, string password, CancellationToken cancellationToken)
	{
		var passwordByteCount = Encoding.UTF8.GetByteCount(password);
		var messageLength = 4 + passwordByteCount + 1;

		var memory = writer.GetMemory(1 + messageLength);
		var span = memory.Span;
		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), messageLength);

		Encoding.UTF8.GetBytes(password, span.Slice(5, passwordByteCount));
		span[5 + passwordByteCount] = 0;

		writer.Advance(1 + messageLength);
		await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	private static async ValueTask SendMd5PasswordAsync(
		PipeWriter writer,
		string username,
		string password,
		ReadOnlyMemory<byte> salt,
		CancellationToken cancellationToken)
	{
		Span<byte> innerHashHex = stackalloc byte[32];
		ComputeUserPasswordMd5Hex(password, username, innerHashHex);

		Span<byte> combined = stackalloc byte[32 + 4];
		innerHashHex.CopyTo(combined);
		salt.Span.CopyTo(combined.Slice(32));

		Span<byte> outerHashHex = stackalloc byte[32];
		ComputeMd5Hex(combined, outerHashHex);

		const int md5ResponseLength = 3 + 32;
		var messageLength = 4 + md5ResponseLength + 1;

		var memory = writer.GetMemory(1 + messageLength);
		var span = memory.Span;
		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), messageLength);

		span[5] = (byte)'m';
		span[6] = (byte)'d';
		span[7] = (byte)'5';

		outerHashHex.CopyTo(span.Slice(8, 32));
		span[8 + 32] = 0;

		writer.Advance(1 + messageLength);
		await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	private static void ComputeUserPasswordMd5Hex(string password, string username, Span<byte> destinationHex)
	{
		var pwdBytesCount = Encoding.UTF8.GetByteCount(password);
		var userBytesCount = Encoding.UTF8.GetByteCount(username);
		var totalLen = pwdBytesCount + userBytesCount;

		byte[]? rented = null;
		Span<byte> buffer = totalLen <= 256
			? stackalloc byte[totalLen]
			: (rented = ArrayPool<byte>.Shared.Rent(totalLen)).AsSpan(0, totalLen);

		try
		{
			Encoding.UTF8.GetBytes(password, buffer);
			Encoding.UTF8.GetBytes(username, buffer.Slice(pwdBytesCount));

			Span<byte> hash = stackalloc byte[16];
			MD5.HashData(buffer, hash);
			ToHexLower(hash, destinationHex);
		}
		finally
		{
			if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ComputeMd5Hex(ReadOnlySpan<byte> source, Span<byte> destinationHex)
	{
		Span<byte> hash = stackalloc byte[16];
		MD5.HashData(source, hash);
		ToHexLower(hash, destinationHex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ToHexLower(ReadOnlySpan<byte> bytes, Span<byte> destinationHex)
	{
		const string hexAlphabet = "0123456789abcdef";
		for (var i = 0; i < bytes.Length; i++)
		{
			destinationHex[i * 2] = (byte)hexAlphabet[bytes[i] >> 4];
			destinationHex[i * 2 + 1] = (byte)hexAlphabet[bytes[i] & 0xF];
		}
	}

	private static void ProcessBackendKeyData(ReadOnlySequence<byte> payload, out int? pid, out int? secret)
	{
		Span<byte> data = stackalloc byte[8];
		payload.Slice(0, 8).CopyTo(data);

		pid = BinaryPrimitives.ReadInt32BigEndian(data.Slice(0, 4));
		secret = BinaryPrimitives.ReadInt32BigEndian(data.Slice(4, 4));
	}

	[DoesNotReturn]
	private static void ThrowUnexpectedEof() =>
		throw new InvalidOperationException("Connection closed by server during authentication handshake.");

	[DoesNotReturn]
	private static void ThrowSaslNotImplemented() =>
		throw new NotImplementedException("SASL/SCRAM authentication mechanism is not supported.");

	[DoesNotReturn]
	private static void ThrowUnsupportedAuthType(AuthenticationType authType) =>
		throw new NotSupportedException($"PostgreSQL authentication type '{authType}' is not supported.");
}