using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.PostgreSQL.Enums;

namespace Ring.PostgreSQL.Extensions;

internal static class PipeWriterExtensions
{
	/// <summary>
	///     Sends a frontend Simple Query ('Q') message asynchronously using pre-encoded UTF-8 SQL bytes.
	/// </summary>
	internal static ValueTask<FlushResult> SendQueryAsync(this PipeWriter writer, ReadOnlyMemory<byte> sql, CancellationToken cancellationToken = default)
	{
		// Code size: 120 (0x78)
		var msgLength = 6 + sql.Length;
		var destination = writer.GetMemory(msgLength);
		var span = destination.Span;

		ref var destRef = ref MemoryMarshal.GetReference(span);
		Unsafe.WriteUnaligned(ref destRef, (byte)FrontendMessageCode.Query);

		var payloadLength = msgLength - 1;
		var bigEndianLen = BinaryPrimitives.ReverseEndianness(payloadLength);
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, 1), bigEndianLen);
		Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref destRef, 5), ref MemoryMarshal.GetReference(sql.Span),	(uint)sql.Length);
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, msgLength - 1), (byte)0);

		writer.Advance(msgLength);
		return writer.FlushAsync(cancellationToken);
	}

	/// <summary>
	///     Sends a frontend Simple Query ('Q') message synchronously.
	/// </summary>
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static void SendQuery(this PipeWriter writer, ReadOnlySpan<byte> sql)
	{
		// Code size: 105 (0x69)
		var msgLength = 6 + sql.Length;
		var destination = writer.GetSpan(msgLength);

		ref var destRef = ref MemoryMarshal.GetReference(destination);
		Unsafe.WriteUnaligned(ref destRef, (byte)FrontendMessageCode.Query);

		var bigEndianLen = BinaryPrimitives.ReverseEndianness(msgLength - 1);
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, 1), bigEndianLen);
		Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref destRef, 5), ref MemoryMarshal.GetReference(sql), (uint)sql.Length);
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, msgLength - 1), (byte)0);

		writer.Advance(msgLength);
		FlushSynchronously(writer);
	}

	/// <summary>
	///     Sends the Extended Query subprotocol for parameterized queries directly via PipeWriter.
	/// </summary>
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static void SendExtendedQuery(this PipeWriter writer, ReadOnlySpan<byte> sql, ReadOnlySpan<byte> variables)
	{
		var parseMsgLength = 9 + sql.Length;
		var totalLength = parseMsgLength + variables.Length;

		var destination = writer.GetSpan(totalLength);
		ref var destRef = ref MemoryMarshal.GetReference(destination);

		var offset = 0;
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, offset++), (byte)FrontendMessageCode.Parse);

		var bigEndianParseLen = BinaryPrimitives.ReverseEndianness(parseMsgLength - 1);
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, offset), bigEndianParseLen);
		offset += 4;

		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, offset++), (byte)0); // unnamed statement
		Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref destRef, offset), ref MemoryMarshal.GetReference(sql),	(uint)sql.Length);
		offset += sql.Length;
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, offset++), (byte)0); // trailing NUL

		var zeroParams = BinaryPrimitives.ReverseEndianness((short)0);
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref destRef, offset), zeroParams); // 0 parameter types

		offset += 2;
		Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref destRef, offset), ref MemoryMarshal.GetReference(variables), (uint)variables.Length);
		
		writer.Advance(totalLength);
		FlushSynchronously(writer);
	}

	#region authentication & startup

	internal static async ValueTask SendStartupAsync(this PipeWriter writer, ConnectionParameters connParameters, CancellationToken cancellationToken = default)
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

		// Calculate total payload length
		var length = 4 + 4 + 1; // Length header (4) + Protocol version (4) + Trailing NUL (1)
		length += Encoding.UTF8.GetByteCount(userParam) + 1 + Encoding.UTF8.GetByteCount(userName) + 1;
		length += Encoding.UTF8.GetByteCount(encParam) + 1 + Encoding.UTF8.GetByteCount(clientEnc) + 1;

		if (dbParam != null && dbName != null)
			length += Encoding.UTF8.GetByteCount(dbParam) + 1 + Encoding.UTF8.GetByteCount(dbName) + 1;

		if (appParam != null && appName != null)
			length += Encoding.UTF8.GetByteCount(appParam) + 1 + Encoding.UTF8.GetByteCount(appName) + 1;

		var destination = writer.GetMemory(length);
		var span = destination.Span;

		BinaryPrimitives.WriteInt32BigEndian(span, length);
		BinaryPrimitives.WriteInt32BigEndian(span[4..], protocolVersion3);

		var offset = 8;
		offset += WriteKeyValuePair(span[offset..], userParam, userName);
		offset += WriteKeyValuePair(span[offset..], encParam, clientEnc);

		if (dbParam != null && dbName != null)
			offset += WriteKeyValuePair(span[offset..], dbParam, dbName);

		if (appParam != null && appName != null)
			offset += WriteKeyValuePair(span[offset..], appParam, appName);

		span[offset] = 0; // Final terminating null byte
		writer.Advance(length);

		await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int WriteKeyValuePair(Span<byte> destination, string key, string value)
		{
			var bytesWritten = Encoding.UTF8.GetBytes(key, destination);
			destination[bytesWritten++] = 0;
			bytesWritten += Encoding.UTF8.GetBytes(value, destination[bytesWritten..]);
			destination[bytesWritten++] = 0;
			return bytesWritten;
		}
	}

	internal static async ValueTask SendPasswordMessageAsync(this PipeWriter writer, string password, CancellationToken cancellationToken = default)
	{
		var length = 4 + Encoding.UTF8.GetByteCount(password) + 1;
		var destination = writer.GetMemory(1 + length);
		var span = destination.Span;

		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span[1..], length);
		var written = Encoding.UTF8.GetBytes(password, span[5..]);
		span[5 + written] = 0;

		writer.Advance(1 + length);
		await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	internal static async ValueTask SendSASLInitialResponseAsync(this PipeWriter writer, string mechanism, byte[] data, CancellationToken cancellationToken = default)
	{
		var mechanismLength = Encoding.UTF8.GetByteCount(mechanism);
		var outerLength = 4 + mechanismLength + 1 + 4 + data.Length;
		var destination = writer.GetMemory(1 + outerLength);
		var span = destination.Span;

		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span[1..], outerLength);

		var offset = 5;
		offset += Encoding.UTF8.GetBytes(mechanism, span[offset..]);
		span[offset++] = 0;
		BinaryPrimitives.WriteInt32BigEndian(span[offset..], data.Length);
		offset += 4;
		data.CopyTo(span[offset..]);

		writer.Advance(1 + outerLength);
		await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	internal static async ValueTask SendSASLResponseAsync(this PipeWriter writer, byte[] data, CancellationToken cancellationToken = default)
	{
		var length = 4 + data.Length;
		var destination = writer.GetMemory(1 + length);
		var span = destination.Span;

		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span[1..], length);
		data.CopyTo(span[5..]);

		writer.Advance(1 + length);
		await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	#endregion

	#region private methods

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void FlushSynchronously(PipeWriter writer)
	{
		// Code size: 64 (0x40)
		var flushTask = writer.FlushAsync();
		if (flushTask.IsCompleted)
		{
			_ = flushTask.GetAwaiter().GetResult();
			return;
		}

		flushTask.AsTask().GetAwaiter().GetResult();
	}

	#endregion

}