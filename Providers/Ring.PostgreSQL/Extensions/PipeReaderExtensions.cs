using Ring.Data;
using Ring.Data.Extensions;
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
		// Code size: 333 (0x14d)
		OperationalError? error = null;

		while (true)
		{
			// Always call ReadAsync instead of TryRead so it waits for new data when an incomplete message is retained in the buffer.
			// Safely handle the ValueTask by checking IsCompleted first, complying with CA2012 / ValueTask consumption rules.
			var vt = reader.ReadAsync();
			var result = vt.IsCompleted	? vt.GetAwaiter().GetResult() : vt.AsTask().GetAwaiter().GetResult();
			var buffer = result.Buffer;
			var consumed = buffer.Start;
			var examined = buffer.End;

			while (buffer.Length >= 5)
			{
				if (!buffer.TryReadHeader(out var rawCode, out var bodyLength))
				{
					reader.AdvanceTo(buffer.Start, buffer.End);
					return error.InvalidProtocol(ResourceHelper.GetMessage(ResourceType.InvalidMessageLength));
				}

				var code = rawCode.ToBackendMessageCode();
				var totalLength = 5 + bodyLength;

				if (buffer.Length < totalLength)
				{
					examined = buffer.End;
					break;
				}

				var nextPosition = buffer.GetPosition(totalLength);

				if (code == BackendMessageCode.ReadyForQuery)
				{
					reader.AdvanceTo(nextPosition, nextPosition);
					return error;
				}

				if (code == BackendMessageCode.ErrorResponse)
				{
					error = buffer.Slice(5, bodyLength).ParseErrorFieldsFromSequence();
				}

				consumed = nextPosition;
				buffer = buffer.Slice(totalLength);
			}

			reader.AdvanceTo(consumed, examined);

			if (result.IsCompleted)
			{
				return error.ConnectionClosed(ResourceHelper.GetMessage(ResourceType.ClosedBeforeReadyForQuery));
			}
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
			var consumed = buffer.Start;
			var examined = buffer.End;

			while (buffer.Length >= 5)
			{
				if (!buffer.TryReadHeader(out var rawCode, out var bodyLength))
				{
					reader.AdvanceTo(buffer.Start, buffer.End);
					return error.InvalidProtocol("Invalid message length received from server.");
				}

				var code = rawCode.ToBackendMessageCode();
				var totalLength = 5 + bodyLength;

				if (buffer.Length < totalLength)
				{
					examined = buffer.End;
					break;
				}

				var nextPosition = buffer.GetPosition(totalLength);

				if (code == BackendMessageCode.ReadyForQuery)
				{
					reader.AdvanceTo(nextPosition, nextPosition);
					return error;
				}

				if (code == BackendMessageCode.ErrorResponse)
				{
					error = buffer.Slice(5, bodyLength).ParseErrorFieldsFromSequence();
				}

				consumed = nextPosition;
				buffer = buffer.Slice(totalLength);
			}

			reader.AdvanceTo(consumed, examined);

			if (result.IsCompleted)
			{
				return error.ConnectionClosed("Connection closed before ReadyForQuery message was received.");
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