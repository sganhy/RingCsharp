using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.PostgreSQL.Enums;

namespace Ring.PostgreSQL.Extensions;

internal static class PipeWriterExtensions
{

	internal static ValueTask<FlushResult> SendQueryAsync(this PipeWriter writer, ReadOnlyMemory<byte> sql, CancellationToken cancellationToken = default)
	{
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
		var msgLength = 6 + sql.Length;
		var span = writer.GetSpan(msgLength);

		span[0] = (byte)FrontendMessageCode.Query;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), msgLength - 1);
		sql.CopyTo(span[5..]);
		span[msgLength - 1] = 0;

		writer.Advance(msgLength);
		writer.FlushBlocking();
	}

	/// <summary>Writes Parse + a pre-built Bind/Execute/Sync payload and flushes (blocking).</summary>
	internal static void SendExtendedQuery(this PipeWriter writer, ReadOnlySpan<byte> sql, ReadOnlySpan<byte> variables)
	{
		var parseMsgLength = 9 + sql.Length;
		var span = writer.GetSpan(parseMsgLength + variables.Length);

		// Parse: 'P', int32 length, unnamed statement \0, sql \0, int16 parameter type count (0)
		span[0] = (byte)FrontendMessageCode.Parse;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), parseMsgLength - 1);
		span[5] = 0;

		sql.CopyTo(span[6..]);
		var offset = 6 + sql.Length;

		span[offset++] = 0;
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(offset, 2), 0);
		offset += 2;

		variables.CopyTo(span[offset..]);

		writer.Advance(offset + variables.Length);
		writer.FlushBlocking();
	}

	/// <summary>
	/// Writes Parse + Bind/Execute/Sync directly into the pipe buffer (no intermediate payload buffer, no extra copy)
	/// and flushes (blocking). Nothing is advanced if building the payload throws, so the pipe stays clean.
	/// </summary>
	internal static void SendExtendedQuery(this PipeWriter writer, ReadOnlySpan<byte> sql, in SaveQuery query, Encoding encoding)
	{
		// Code size: 155 (0x9b)
		var parseMsgLength = 9 + sql.Length;
		var span = writer.GetSpan(parseMsgLength + query.GetVariablesPayloadSize(encoding));

		// Parse: 'P', int32 length, unnamed statement \0, sql \0, int16 parameter type count (0)
		span[0] = (byte)FrontendMessageCode.Parse;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), parseMsgLength - 1);
		span[5] = 0;

		sql.CopyTo(span[6..]);
		var offset = 6 + sql.Length;

		span[offset++] = 0;
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(offset, 2), 0);
		offset += 2;

		// Bind + Execute + Sync
		offset += query.WriteVariablesPayload(span[offset..], encoding);

		writer.Advance(offset);
		writer.FlushBlocking();
	}

	#region authentication & startup

	internal static ValueTask<FlushResult> SendStartupAsync(this PipeWriter writer, ConnectionParameters connParameters, CancellationToken cancellationToken = default)
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

		return writer.FlushAsync(cancellationToken);

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
	/// Sends a PasswordMessage (cleartext password or the "md5..." hash) and zeroes the bytes
	/// in the pipe buffer once the flush has completed (or failed).
	/// </summary>
	internal static ValueTask SendPasswordMessageAsync(this PipeWriter writer, string password, CancellationToken cancellationToken = default)
	{
		var length = 4 + Encoding.UTF8.GetByteCount(password) + 1;
		var total = 1 + length;

		var memory = writer.GetMemory(total).Slice(0, total);
		var span = memory.Span;

		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span[1..], length);
		var written = Encoding.UTF8.GetBytes(password, span[5..]);
		span[5 + written] = 0;

		writer.Advance(total);
		return FlushAndScrubAsync(writer, memory, cancellationToken);
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

	/// <summary>Sends the SCRAM client-final message (carries the client proof) and zeroes it in the pipe buffer afterwards.</summary>
	internal static ValueTask SendSASLResponseAsync(this PipeWriter writer, byte[] data, CancellationToken cancellationToken = default)
	{
		var length = 4 + data.Length;
		var total = 1 + length;

		var memory = writer.GetMemory(total).Slice(0, total);
		var span = memory.Span;

		span[0] = (byte)FrontendMessageCode.Password;
		BinaryPrimitives.WriteInt32BigEndian(span[1..], length);
		data.CopyTo(span[5..]);

		writer.Advance(total);
		return FlushAndScrubAsync(writer, memory, cancellationToken);
	}

	#endregion

	#region private helpers

	// Assumes a stream-backed writer (PipeWriter.Create(stream)): when FlushAsync completes, the bytes have
	// been handed to the stream and the writer no longer reads this region, so it can be zeroed safely.
	// Do NOT use with a Pipe-backed writer, where a reader may still be consuming the memory.
	private static async ValueTask FlushAndScrubAsync(PipeWriter writer, Memory<byte> written, CancellationToken cancellationToken)
	{
		try
		{
			await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			CryptographicOperations.ZeroMemory(written.Span);
		}
	}

	/// <summary>Flushes and blocks. Allocation-free when the flush completes synchronously (the usual case for socket sends).</summary>
	internal static void FlushBlocking(this PipeWriter writer)
	{
		var flush = writer.FlushAsync();
		if (flush.IsCompleted)
			flush.GetAwaiter().GetResult();
		else
			flush.AsTask().GetAwaiter().GetResult();
	}

	#endregion
}
