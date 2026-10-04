using Ring.Data;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Models;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class PipeReaderExtensions
{
	private const int InitialRowCapacityHint = 16;
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly string BooleanTrue = true.ToString(DefaultCulture);
	private static readonly string BooleanFalse = false.ToString(DefaultCulture);
	private const byte PgTrueChar = (byte)'t';
	private const byte PgTrueDigit = (byte)'1';

	internal static async ValueTask<MessageSlice> ReadMessageAsync(this PipeReader reader, bool errorOnly, CancellationToken cancellationToken = default)
	{
		while (true)
		{
			var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
			var buffer = result.Buffer;

			if (buffer.Length < 5)
			{
				reader.AdvanceTo(buffer.Start, buffer.End);
				if (result.IsCompleted) ThrowConnectionWasClosedByServer();
				continue;
			}

			if (!TryReadHeader(ref buffer, out var code, out var bodyLength))
				ThrowInvalidMessageLength();

			var totalLength = 5 + bodyLength;
			if (buffer.Length < totalLength)
			{
				reader.AdvanceTo(buffer.Start, buffer.End);
				if (result.IsCompleted) ThrowConnectionWasClosedByServer();
				continue;
			}

			var bodySequence = (errorOnly && code != (byte)BackendMessageCode.ErrorResponse) || bodyLength <= 0
				? ReadOnlySequence<byte>.Empty
				: buffer.Slice(5, bodyLength);

			var totalPosition = buffer.GetPosition(totalLength);
			return new MessageSlice(code, bodySequence, totalPosition);
		}
	}

	internal static async ValueTask<(OperationalError? Error, byte TransactionStatus)> DrainToReadyForQueryAsync(this PipeReader reader, CancellationToken cancellationToken = default)
	{
		OperationalError? operationalError = null;
		while (true)
		{
			var msg = await reader.ReadMessageAsync(true, cancellationToken).ConfigureAwait(false);

			if (msg.Code == (byte)BackendMessageCode.ReadyForQuery)
			{
				var txStatus = !msg.Body.IsEmpty ? msg.Body.FirstSpan[0] : (byte)0;
				reader.AdvanceTo(msg.EndPosition);
				return (operationalError, txStatus);
			}

			if (msg.Code == (byte)BackendMessageCode.ErrorResponse)
			{
				byte drainCode;
				byte txStatus = 0;
				operationalError = ParseErrorFieldsFromSequence(msg.Body);
				reader.AdvanceTo(msg.EndPosition);

				do
				{
					var drainMsg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
					drainCode = drainMsg.Code;
					if (drainCode == (byte)BackendMessageCode.ReadyForQuery && !drainMsg.Body.IsEmpty)
					{
						txStatus = drainMsg.Body.FirstSpan[0];
					}
					reader.AdvanceTo(drainMsg.EndPosition);
				}
				while (drainCode != (byte)BackendMessageCode.ReadyForQuery);

				return (operationalError, txStatus);
			}

			reader.AdvanceTo(msg.EndPosition);
		}
	}

	internal static async ValueTask<(int? BackendPid, int? BackendSecret)> WaitUntilReadyAsync(this PipeReader reader, CancellationToken cancellationToken = default)
	{
		int? pid = null;
		int? secret = null;
		while (true)
		{
			var msg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);

			switch ((BackendMessageCode)msg.Code)
			{
				case BackendMessageCode.BackendKeyData:
					if (msg.Body.Length >= 8)
					{
						ParseKeyData(msg.Body, out pid, out secret);
					}
					reader.AdvanceTo(msg.EndPosition);
					continue;

				case BackendMessageCode.ParameterStatus:
				case BackendMessageCode.NoticeResponse:
					reader.AdvanceTo(msg.EndPosition);
					continue;

				case BackendMessageCode.ReadyForQuery:
					reader.AdvanceTo(msg.EndPosition);
					return (pid, secret);

				case BackendMessageCode.ErrorResponse:
					var error = ParseErrorFieldsFromSequence(msg.Body).ToPgOperationalError();
					reader.AdvanceTo(msg.EndPosition);
					throw error;

				default:
					reader.AdvanceTo(msg.EndPosition);
					continue;
			}
		}
	}

	internal static async ValueTask<string?[]> ReadRetrieveRecordsAsync(this PipeReader reader, Encoding encoding, Table table, Action<byte>? onTransactionStatus = null, int rowCount = -1, CancellationToken cancellationToken = default)
	{
		var pool = ArrayPool<string?>.Shared;
		var initialCapacity = table.RecordSize * (rowCount > 0 ? rowCount : InitialRowCapacityHint);
		var buffer = pool.Rent(initialCapacity);
		var count = 0;

		try
		{
			while (true)
			{
				var msg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);

				switch (msg.Code)
				{
					case (byte)BackendMessageCode.DataRow:
						AppendRecordData(msg.Body, encoding, pool, table, ref buffer, count);
						count += table.RecordSize;
						reader.AdvanceTo(msg.EndPosition);
						break;

					case (byte)BackendMessageCode.ReadyForQuery:
						{
							if (!msg.Body.IsEmpty)
								onTransactionStatus?.Invoke(msg.Body.FirstSpan[0]);

							var results = new string?[count];
							Array.Copy(buffer, results, count);
							reader.AdvanceTo(msg.EndPosition);
							return results;
						}

					case (byte)BackendMessageCode.ErrorResponse:
						{
							byte drainCode;
							var error = ParseErrorFieldsFromSequence(msg.Body);
							reader.AdvanceTo(msg.EndPosition);

							do
							{
								var drainMsg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
								drainCode = drainMsg.Code;
								if (drainCode == (byte)BackendMessageCode.ReadyForQuery && !drainMsg.Body.IsEmpty)
								{
									onTransactionStatus?.Invoke(drainMsg.Body.FirstSpan[0]);
								}
								reader.AdvanceTo(drainMsg.EndPosition);
							}
							while (drainCode != (byte)BackendMessageCode.ReadyForQuery);

							throw error.ToPgOperationalError();
						}

					case (byte)BackendMessageCode.RowDescription:
					case (byte)BackendMessageCode.CommandComplete:
					case (byte)BackendMessageCode.EmptyQueryResponse:
					case (byte)BackendMessageCode.NoticeResponse:
					case (byte)BackendMessageCode.ParameterStatus:
					case (byte)BackendMessageCode.NotificationResponse:
					case (byte)BackendMessageCode.ParseComplete:
					case (byte)BackendMessageCode.BindComplete:
					case (byte)BackendMessageCode.NoData:
					case (byte)BackendMessageCode.ParameterDescription:
						reader.AdvanceTo(msg.EndPosition);
						break;

					default:
						reader.AdvanceTo(msg.EndPosition);
						UnexpectedProviderMessage(msg.Code);
						break;
				}
			}
		}
		finally
		{
			pool.Return(buffer, clearArray: true);
		}
	}

	#region private methods

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool TryReadHeader(ref ReadOnlySequence<byte> buffer, out byte code, out int bodyLength)
	{
		if (buffer.Length < 5)
		{
			code = default;
			bodyLength = default;
			return false;
		}

		var firstSpan = buffer.FirstSpan;
		if (firstSpan.Length >= 5)
		{
			code = firstSpan[0];
			bodyLength = BinaryPrimitives.ReadInt32BigEndian(firstSpan.Slice(1, 4)) - 4;
			return bodyLength >= 0;
		}

		Span<byte> header = stackalloc byte[5];
		buffer.Slice(0, 5).CopyTo(header);
		code = header[0];
		bodyLength = BinaryPrimitives.ReadInt32BigEndian(header[1..]) - 4;
		return bodyLength >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ParseKeyData(ReadOnlySequence<byte> body, out int? pid, out int? secret)
	{
		Span<byte> temp = stackalloc byte[8];
		body.Slice(0, 8).CopyTo(temp);
		pid = BinaryPrimitives.ReadInt32BigEndian(temp.Slice(0, 4));
		secret = BinaryPrimitives.ReadInt32BigEndian(temp.Slice(4, 4));
	}

	private static void AppendRecordData(ReadOnlySequence<byte> bodySequence, Encoding encoding, ArrayPool<string?> pool, Table table, ref string?[] buffer, int count)
	{
		var required = count + table.RecordSize;
		if (required > buffer.Length) EnsureCapacity(pool, ref buffer, count, required);

		var seqReader = new SequenceReader<byte>(bodySequence);
		seqReader.Advance(2); // Skip column count field

		var columns = new ReadOnlySpan<Column>(table.Columns);

		foreach (var column in columns)
		{
			if (column.Type == EntityType.SearchableColumn) continue;

			if (!seqReader.TryReadBigEndian(out int valueLength))
				break;

			var index = column.RecordIndex + count;

			if (valueLength < 0)
			{
				buffer[index] = null;
				continue;
			}

			if (column.Type == EntityType.TimeZoneColumn)
			{
				seqReader.Advance(valueLength);
				continue;
			}

			var valueSeq = seqReader.Sequence.Slice(seqReader.Position, valueLength);
			seqReader.Advance(valueLength);

			if (column.FieldType == FieldType.ByteArray)
			{
				buffer[index] = GetByteaStringFromSequence(valueSeq, valueLength);
			}
			else if (column.FieldType == FieldType.Boolean)
			{
				var firstByte = valueSeq.FirstSpan[0];
				var isTrue = valueLength == 1 && (firstByte == PgTrueChar || firstByte == PgTrueDigit);
				buffer[index] = isTrue ? BooleanTrue : BooleanFalse;
			}
			else
			{
				buffer[index] = GetStringFromSequence(valueSeq, encoding);
			}
		}
		buffer[count + table.RecordSize - 1] = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string GetByteaStringFromSequence(ReadOnlySequence<byte> sequence, int length)
	{
		if (sequence.IsSingleSegment)
		{
			return sequence.FirstSpan.ParseByteaHexToBase64(0, length);
		}

		byte[]? rented = null;
		Span<byte> span = length <= 128
			? stackalloc byte[length]
			: (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length);

		try
		{
			sequence.CopyTo(span);
			return span.ParseByteaHexToBase64(0, length);
		}
		finally
		{
			if (rented != null)
				ArrayPool<byte>.Shared.Return(rented);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string GetStringFromSequence(ReadOnlySequence<byte> sequence, Encoding encoding)
	{
		if (sequence.IsSingleSegment) return encoding.GetString(sequence.FirstSpan);

		var length = (int)sequence.Length;
		byte[]? rented = null;
		Span<byte> span = length <= 128 ? stackalloc byte[length] : (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length);

		try
		{
			sequence.CopyTo(span);
			return encoding.GetString(span);
		}
		finally
		{
			if (rented != null)
				ArrayPool<byte>.Shared.Return(rented);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static OperationalError ParseErrorFieldsFromSequence(ReadOnlySequence<byte> sequence)
	{
		if (sequence.IsSingleSegment)
		{
			return sequence.FirstSpan.ParseErrorFields();
		}

		var length = (int)sequence.Length;
		byte[]? rented = null;
		Span<byte> span = length <= 128 ? stackalloc byte[length] : (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length);

		try
		{
			sequence.CopyTo(span);
			return span.ParseErrorFields();
		}
		finally
		{
			if (rented != null)
				ArrayPool<byte>.Shared.Return(rented);
		}
	}

	private static void EnsureCapacity(ArrayPool<string?> pool, ref string?[] buffer, int usedCount, int required)
	{
		var grown = pool.Rent(Math.Max(buffer.Length * 2, required));
		Array.Copy(buffer, grown, usedCount);
		pool.Return(buffer, clearArray: true);
		buffer = grown;
	}

	[DoesNotReturn]
	private static void ThrowConnectionWasClosedByServer() =>
		throw new EndOfStreamException(ResourceHelper.GetMessage(ResourceType.ConnectionClosedByServer));

	[DoesNotReturn]
	private static void UnexpectedProviderMessage(byte code) =>
		throw new InvalidOperationException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.UnexpectedProviderMessage), (char)code));

	[DoesNotReturn]
	private static void ThrowInvalidMessageLength() =>
		throw new InvalidOperationException(ResourceHelper.GetMessage(ResourceType.InvalidMessageLengthFromServer));

	#endregion
}