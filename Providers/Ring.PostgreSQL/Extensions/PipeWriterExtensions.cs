using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.PostgreSQL.Enums;

namespace Ring.PostgreSQL.Extensions;

internal static class PipeWriterExtensions
{
	/// <summary>
	/// Hard protocol ceiling for the SQL text: PostgreSQL rejects any message above 1 GB - 1 (the length field counts its own 4 bytes),
	/// and the Query/Parse message adds a code byte, the length and a terminator. Kept conservative. Single source of truth:
	/// <see cref="Connection"/> derives its (lower) driver limit from it.
	/// </summary>
	internal const int MaxSqlLength = 0x3FFFFFFF - 16; // 1,073,741,807
	private const int StackallocThreshold = 128;

	internal static ValueTask<FlushResult> SendQueryAsync(this PipeWriter writer, ReadOnlyMemory<byte> sql, CancellationToken cancellationToken = default)
	{
		EnsureSqlLength(sql.Length);
		var msgLength = 6 + sql.Length;
		var span = writer.GetSpan(msgLength);

		span[0] = (byte)FrontendMessageCode.Query;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), msgLength - 1);
		sql.Span.CopyTo(span[5..]);
		span[msgLength - 1] = 0;

		writer.Advance(msgLength);
		return writer.FlushAsync(cancellationToken);
	}

	/// <summary>Writes a Query message and flushes (blocking).</summary>
	internal static void SendQuery(this PipeWriter writer, ReadOnlySpan<byte> sql)
	{
		EnsureSqlLength(sql.Length);
		var msgLength = 6 + sql.Length;
		var span = writer.GetSpan(msgLength);

		span[0] = (byte)FrontendMessageCode.Query;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), msgLength - 1);
		sql.CopyTo(span[5..]);
		span[msgLength - 1] = 0;

		writer.Advance(msgLength);
		writer.FlushBlocking();
	}

	/// <summary>
	/// Writes Parse + Bind/Execute/Sync directly into the pipe buffer (no intermediate payload buffer, no extra copy)
	/// and flushes (blocking). Nothing is advanced if building the payload throws, so the pipe stays clean.
	/// </summary>
	internal static void SendExtendedQuery(this PipeWriter writer, ReadOnlySpan<byte> sql, in SaveQuery query, Encoding encoding)
	{
		EnsureSqlLength(sql.Length);
		var parseMsgLength = 9 + sql.Length;
		var variablesPayloadSize = query.GetVariablesPayloadSize(encoding);
		var totalMsgLength = EnsureMessageLength((long)parseMsgLength + variablesPayloadSize);

		var span = writer.GetSpan(totalMsgLength);

		// Parse: 'P', int32 length, unnamed statement \0, sql \0, int16 parameter type count (0)
		span[0] = (byte)FrontendMessageCode.Parse;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), parseMsgLength - 1);
		span[5] = 0;

		sql.CopyTo(span[6..]);
		var offset = 6 + sql.Length;

		span[offset++] = 0;
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(offset, 2), 0);
		offset += 2;

		// Write variables payload directly into the remaining pipe buffer space
		offset += query.WriteVariablesPayload(span[offset..], encoding);

		writer.Advance(offset);
		writer.FlushBlocking();
	}

	internal static ValueTask<FlushResult> SendStartupAsync(this PipeWriter writer, ConnectionParameters connParameters, CancellationToken cancellationToken = default)
	{
		WriteStartup(writer, connParameters);
		return writer.FlushAsync(cancellationToken);
	}

	/// <summary>Writes the StartupMessage and flushes (blocking).</summary>
	internal static void SendStartup(this PipeWriter writer, ConnectionParameters connParameters)
	{
		WriteStartup(writer, connParameters);
		writer.FlushBlocking();
	}

	// The message building is shared by SendStartupAsync and SendStartup: only the flush differs.
	private static void WriteStartup(PipeWriter writer, ConnectionParameters connParameters)
	{
		const int protocolVersion3 = 0x00030000;

		var userParam = connParameters.GetParameterName(ConnectionParametersType.UserName);
		var userName = connParameters.UserName;

		var encParam = connParameters.GetParameterName(ConnectionParametersType.ClientEncoding);
		var clientEnc = connParameters.ClientEncoding;

		var dbParam = !string.IsNullOrEmpty(connParameters.DatabaseName)
			? connParameters.GetParameterName(ConnectionParametersType.DataBase)
			: null;
		var dbName = connParameters.DatabaseName;

		var appParam = !string.IsNullOrEmpty(connParameters.ApplicationName)
			? connParameters.GetParameterName(ConnectionParametersType.ApplicationName)
			: null;
		var appName = connParameters.ApplicationName;

		var length = 4 + 4 + 1;
		length += Encoding.UTF8.GetByteCount(userParam) + 1 + Encoding.UTF8.GetByteCount(userName) + 1;
		length += Encoding.UTF8.GetByteCount(encParam) + 1 + Encoding.UTF8.GetByteCount(clientEnc) + 1;

		if (dbParam != null && dbName != null)
			length += Encoding.UTF8.GetByteCount(dbParam) + 1 + Encoding.UTF8.GetByteCount(dbName) + 1;

		if (appParam != null && appName != null)
			length += Encoding.UTF8.GetByteCount(appParam) + 1 + Encoding.UTF8.GetByteCount(appName) + 1;

		var span = writer.GetSpan(length);

		BinaryPrimitives.WriteInt32BigEndian(span, length);
		BinaryPrimitives.WriteInt32BigEndian(span[4..], protocolVersion3);

		var offset = 8;
		offset += WriteKeyValuePair(span[offset..], userParam, userName);
		offset += WriteKeyValuePair(span[offset..], encParam, clientEnc);

		if (dbParam != null && dbName != null)
			offset += WriteKeyValuePair(span[offset..], dbParam, dbName);

		if (appParam != null && appName != null)
			offset += WriteKeyValuePair(span[offset..], appParam, appName);

		span[offset] = 0;
		writer.Advance(length);

		static int WriteKeyValuePair(Span<byte> destination, string key, string value)
		{
			var bytesWritten = Encoding.UTF8.GetBytes(key, destination);
			destination[bytesWritten++] = 0;
			bytesWritten += Encoding.UTF8.GetBytes(value, destination[bytesWritten..]);
			destination[bytesWritten++] = 0;
			return bytesWritten;
		}
	}

	/// <summary>
	/// Sends a PasswordMessage (cleartext password or the "md5..." hash). The pipe buffer is never touched after <c>Advance</c>
	/// (the writer owns it); it is scrubbed by <see cref="ScrubbingMemoryPool"/> when the writer releases it.
	/// </summary>
	internal static ValueTask SendPasswordMessageAsync(this PipeWriter writer, string password, CancellationToken cancellationToken = default)
	{
		var length = 4 + Encoding.UTF8.GetByteCount(password) + 1;
		var total = 1 + length;

		var span = writer.GetSpan(total);

		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span[1..], length);
		var written = Encoding.UTF8.GetBytes(password, span[5..]);
		span[5 + written] = 0;

		writer.Advance(total);
		return FlushDiscardResultAsync(writer, cancellationToken);
	}

	/// <summary>
	/// Sends the MD5 PasswordMessage: "md5" + hex(md5(hex(md5(password + user)) + salt)).
	/// Built directly in the pipe buffer (no string allocation); the intermediate hashes on the stack are zeroed afterwards,
	/// including when an exception is thrown half way. The copy in the pipe buffer is scrubbed by <see cref="ScrubbingMemoryPool"/>.
	/// </summary>
	internal static ValueTask SendMd5PasswordAsync(this PipeWriter writer, string username, string password, ReadOnlySpan<byte> salt, CancellationToken cancellationToken = default)
	{
		if (salt.Length != 4) throw new ArgumentException("The MD5 salt must be 4 bytes.", nameof(salt));

		Span<byte> innerHashHex = stackalloc byte[32];
		Span<byte> combined = stackalloc byte[32 + 4];
		Span<byte> outerHashHex = stackalloc byte[32];

		try
		{
			ComputeUserPasswordMd5Hex(password, username, innerHashHex);

			innerHashHex.CopyTo(combined);
			salt.CopyTo(combined[32..]);

			ComputeMd5Hex(combined, outerHashHex);

			const int length = 4 + 3 + 32 + 1;
			const int total = 1 + length;

			var span = writer.GetSpan(total);

			span[0] = (byte)FrontendMessageCode.Password;
			BinaryPrimitives.WriteInt32BigEndian(span[1..], length);
			span[5] = (byte)'m';
			span[6] = (byte)'d';
			span[7] = (byte)'5';
			outerHashHex.CopyTo(span.Slice(8, 32));
			span[8 + 32] = 0;

			writer.Advance(total);

			return FlushDiscardResultAsync(writer, cancellationToken);
		}
		finally
		{
			// inner hash == md5(password + user) is itself enough to authenticate: don't leave it on the stack
			CryptographicOperations.ZeroMemory(innerHashHex);
			CryptographicOperations.ZeroMemory(combined);
			CryptographicOperations.ZeroMemory(outerHashHex);
		}
	}

	internal static ValueTask<FlushResult> SendSASLInitialResponseAsync(this PipeWriter writer, string mechanism, byte[] data, CancellationToken cancellationToken = default)
	{
		var mechanismLength = Encoding.UTF8.GetByteCount(mechanism);
		var outerLength = 4 + mechanismLength + 1 + 4 + data.Length;
		var span = writer.GetSpan(1 + outerLength);

		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span[1..], outerLength);

		var offset = 5;
		offset += Encoding.UTF8.GetBytes(mechanism, span[offset..]);
		span[offset++] = 0;
		BinaryPrimitives.WriteInt32BigEndian(span[offset..], data.Length);
		offset += 4;
		data.CopyTo(span[offset..]);

		writer.Advance(1 + outerLength);
		return writer.FlushAsync(cancellationToken);
	}

	/// <summary>Sends the SCRAM client-final message (carries the client proof); the pipe buffer is scrubbed by <see cref="ScrubbingMemoryPool"/> (the caller should zero its own <paramref name="data"/> array).</summary>
	internal static ValueTask SendSASLResponseAsync(this PipeWriter writer, byte[] data, CancellationToken cancellationToken = default)
	{
		var length = 4 + data.Length;
		var total = 1 + length;

		var span = writer.GetSpan(total);

		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span[1..], length);
		data.CopyTo(span[5..]);

		writer.Advance(total);
		return FlushDiscardResultAsync(writer, cancellationToken);
	}

	#region private helpers

	// Flush for callers that only need to know when the bytes are out. Nothing is zeroed here on purpose: after Advance the memory belongs
	// to the writer (it may still be handed to the stream, kept as the tail segment, or returned to a pool), so scrubbing is the job of the
	// pool the writer was created with (see ScrubbingMemoryPool), which zeroes a buffer exactly when it is released.
	private static async ValueTask FlushDiscardResultAsync(PipeWriter writer, CancellationToken cancellationToken)
		=> await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void EnsureSqlLength(int length)
	{
		if ((uint)length > MaxSqlLength) ThrowSqlTooLong(length);
	}

	// Guards the int arithmetic of the extended-query message (Parse + Bind/Execute/Sync) and the size of the single GetSpan request.
	private static int EnsureMessageLength(long length)
	{
		if ((ulong)length > (ulong)Array.MaxLength) ThrowMessageTooLong(length);
		return (int)length;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowSqlTooLong(int length)
	{
		throw new ArgumentOutOfRangeException("sql", length, $"The SQL text is {length} bytes; the PostgreSQL protocol allows at most {MaxSqlLength}.");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowMessageTooLong(long length)
	{
		throw new ArgumentOutOfRangeException("query", length, "The extended query message is too large to be built in a single buffer.");
	}

	private static void ComputeUserPasswordMd5Hex(string password, string username, Span<byte> destinationHex)
	{
		var pwdBytesCount = Encoding.UTF8.GetByteCount(password);
		var userBytesCount = Encoding.UTF8.GetByteCount(username);
		var totalLen = pwdBytesCount + userBytesCount;

		byte[]? rented = null;
		Span<byte> buffer = totalLen <= StackallocThreshold
			? stackalloc byte[totalLen]
			: (rented = ArrayPool<byte>.Shared.Rent(totalLen)).AsSpan(0, totalLen);

		try
		{
			Encoding.UTF8.GetBytes(password, buffer);
			Encoding.UTF8.GetBytes(username, buffer.Slice(pwdBytesCount));
			Span<byte> hash = stackalloc byte[16];
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
			MD5.HashData(buffer, hash);
#pragma warning restore CA5351
			ToHexLower(hash, destinationHex);
		}
		finally
		{
			CryptographicOperations.ZeroMemory(buffer); // contains the clear password
			if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ComputeMd5Hex(ReadOnlySpan<byte> source, Span<byte> destinationHex)
	{
		Span<byte> hash = stackalloc byte[16];
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
		MD5.HashData(source, hash);
#pragma warning restore CA5351
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

	/// <summary>Flushes and blocks. Allocation-free when the flush completes synchronously (the usual case for socket sends).</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void FlushBlocking(this PipeWriter writer)
	{
		// Code size: 49 (0x31)
		var flush = writer.FlushAsync();
		if (flush.IsCompleted) flush.GetAwaiter().GetResult();
		else WaitSlow(flush);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void WaitSlow(ValueTask<FlushResult> flush) => flush.AsTask().GetAwaiter().GetResult();

	#endregion
}
