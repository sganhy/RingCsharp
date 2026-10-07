using Ring.Data;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Models;
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

			if (!buffer.TryReadHeader(out var code, out var bodyLength))
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

	internal static OperationalError? DrainToReadyForQuery(this PipeReader reader)
	{
		// Code size: 239 (0xef)
		OperationalError? error = null;

		while (true)
		{
			// Fast path: data already buffered, no await machinery at all.
			if (!reader.TryRead(out var result))
			{
				// Nothing buffered yet: block until the socket delivers something.
				var vt = reader.ReadAsync();
				result = vt.IsCompleted
					? vt.GetAwaiter().GetResult()
					: vt.AsTask().GetAwaiter().GetResult();
			}

			var buffer = result.Buffer;

			while (buffer.Length >= 5)
			{
				if (!buffer.TryReadHeader(out var code, out var bodyLength))
					ThrowInvalidMessageLength(); // corrupt length field

				var total = 5 + bodyLength;
				if (buffer.Length < total) break; // incomplete message, wait for more data

				switch ((BackendMessageCode)code)
				{
					case BackendMessageCode.ReadyForQuery:
						reader.AdvanceTo(buffer.GetPosition(total));
						return error;

					case BackendMessageCode.ErrorResponse:
						error = buffer.Slice(5, bodyLength).ParseErrorFieldsFromSequence();
						break;
				}

				buffer = buffer.Slice(total);
			}

			// consumed = what we processed, examined = everything we saw (so TryRead/ReadAsync only return new data)
			reader.AdvanceTo(buffer.Start, buffer.End);

			if (result.IsCompleted)
				throw new InvalidOperationException("Connection closed before ReadyForQuery message was received.");
		}
	}
	internal static async ValueTask<(OperationalError? Error, byte TransactionStatus)> DrainToReadyForQueryAsync(this PipeReader reader, CancellationToken cancellationToken = default)
	{
		OperationalError? error = null;
		byte transactionStatus = 0;

		while (true)
		{
			var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
			var buffer = result.Buffer;

			while (TryReadMessageHeader(ref buffer, out var messageCode, out var payloadLength))
			{
				if (buffer.Length < 5 + payloadLength)
				{
					// Incomplete message body in buffer, wait for more data
					break;
				}

				var payload = buffer.Slice(5, payloadLength);

				switch (messageCode)
				{
					case BackendMessageCode.ReadyForQuery:
						if (!payload.IsEmpty)
						{
							// ReadyForQuery payload contains 1 byte representing transaction status
							transactionStatus = payload.FirstSpan[0];
						}
						var readyPosition = buffer.GetPosition(5 + payloadLength);
						reader.AdvanceTo(readyPosition, readyPosition);
						return (error, transactionStatus);

					case BackendMessageCode.ErrorResponse:
						var errorSpan = payload.IsSingleSegment ? payload.FirstSpan : payload.ToArray().AsSpan();
						error = errorSpan.ParseErrorFields();
						break;

					default:
						// Skip non-error, non-ReadyForQuery payload bytes
						break;
				}

				buffer = buffer.Slice(buffer.GetPosition(5 + payloadLength));
			}

			reader.AdvanceTo(buffer.Start, buffer.End);

			if (result.IsCompleted)
			{
				throw new InvalidOperationException("Connection closed before ReadyForQuery message was received.");
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool TryReadMessageHeader(ref ReadOnlySequence<byte> buffer, out BackendMessageCode code, out int payloadLength)
	{
		if (buffer.Length < 5)
		{
			code = default;
			payloadLength = 0;
			return false;
		}

		Span<byte> header = stackalloc byte[5];
		buffer.Slice(0, 5).CopyTo(header);

		code = (BackendMessageCode)header[0];
		payloadLength = BinaryPrimitives.ReadInt32BigEndian(header.Slice(1, 4)) - 4;
		return true;
	}

	// Refactored to accept a transaction status out reference instead of allocating an Action<byte> closure
	internal static async ValueTask<string?[]> ReadRetrieveRecordsAsync(this PipeReader reader, Encoding encoding, Table table, int rowCount = -1, CancellationToken cancellationToken = default)
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
						msg.Body.AppendRecordData(encoding, table, ref buffer, count);
						count += table.RecordSize;
						reader.AdvanceTo(msg.EndPosition);
						break;

					case (byte)BackendMessageCode.ReadyForQuery:
						{
							var results = new string?[count];
							Array.Copy(buffer, results, count);
							reader.AdvanceTo(msg.EndPosition);
							return results;
						}

					case (byte)BackendMessageCode.ErrorResponse:
						{
							byte drainCode;
							var error = msg.Body.ParseErrorFieldsFromSequence();
							reader.AdvanceTo(msg.EndPosition);

							do
							{
								var drainMsg = await reader.ReadMessageAsync(false, cancellationToken).ConfigureAwait(false);
								drainCode = drainMsg.Code;
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