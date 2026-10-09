using Ring.Data;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Models;
using Ring.Schema.Models;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Buffers;
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
				if (!buffer.TryReadHeader(out var rawCode, out var bodyLength))
					ThrowInvalidMessageLength(); // corrupt length field
				var code = rawCode.ToBackendMessageCode();
				var total = 5 + bodyLength;
				if (buffer.Length < total) break; // incomplete message, wait for more data

				switch (code)
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
	/// <summary>
	/// Reads from the pipe until a <see cref="BackendMessageCode.ReadyForQuery"/> message is encountered.
	/// Accumulates operational errors and extracts final transaction status using zero-allocation sequence extensions.
	/// </summary>
	internal static async ValueTask<OperationalError?> DrainToReadyForQueryAsync(this PipeReader reader, CancellationToken cancellationToken = default)
	{
		OperationalError? error = null;

		while (true)
		{
			var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
			var buffer = result.Buffer;

			while (buffer.TryReadHeader(out byte rawCode, out int bodyLength))
			{
				var code = rawCode.ToBackendMessageCode();
				var totalMessageLength = 5 + bodyLength;
				if (buffer.Length < totalMessageLength)
				{
					// Incomplete message body in buffer, wait for more network bytes
					break;
				}

				var payload = buffer.Slice(5, bodyLength);

				switch (code)
				{
					case BackendMessageCode.ReadyForQuery:
						if (!payload.IsEmpty)
						{
							// ReadyForQuery payload contains 1 byte representing transaction status ('I', 'T', 'E')
							_ = payload.FirstSpan[0];
						}
						var readyPosition = buffer.GetPosition(totalMessageLength);
						reader.AdvanceTo(readyPosition, readyPosition);
						return error;

					case BackendMessageCode.ErrorResponse:
						// Zero-allocation error field parsing directly from ReadOnlySequence<byte>
						error = payload.ParseErrorFieldsFromSequence();
						break;

					default:
						// Skip non-error, non-ReadyForQuery payload bytes
						break;
				}

				buffer = buffer.Slice(totalMessageLength);
			}

			reader.AdvanceTo(buffer.Start, buffer.End);

			if (result.IsCompleted)
			{
				throw new InvalidOperationException("Connection closed before ReadyForQuery message was received.");
			}
		}
	}


	// Refactored to accept a transaction status out reference instead of allocating an Action<byte> closure
	[AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
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
				var code = msg.Code.ToBackendMessageCode();
				switch (code)
				{
					case BackendMessageCode.DataRow:
						msg.Body.AppendRecordData(encoding, table, ref buffer, count);
						count += table.RecordSize;
						reader.AdvanceTo(msg.EndPosition);
						break;
					case BackendMessageCode.ReadyForQuery:
						{
							var results = new string?[count];
							Array.Copy(buffer, results, count);
							reader.AdvanceTo(msg.EndPosition);
							return results;
						}

					case BackendMessageCode.ErrorResponse:
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
							break;
						}
					case BackendMessageCode.RowDescription:
					case BackendMessageCode.CommandComplete:
					case BackendMessageCode.EmptyQueryResponse:
					case BackendMessageCode.NoticeResponse:
					case BackendMessageCode.ParameterStatus:
					case BackendMessageCode.NotificationResponse:
					case BackendMessageCode.ParseComplete:
					case BackendMessageCode.BindComplete:
					case BackendMessageCode.NoData:
					case BackendMessageCode.ParameterDescription:
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