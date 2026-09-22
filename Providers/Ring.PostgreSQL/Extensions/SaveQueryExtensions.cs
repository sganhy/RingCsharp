using Ring.Data.Models;
using Ring.PostgreSQL.Enums;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class SaveQueryExtensions
{
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private static readonly string BooleanTrue = true.ToString(DefaultCulture);

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static int GetVariablesPayloadSize(this in SaveQuery query, Encoding encoding)
	{
		ReadOnlySpan<Column> columns = query.Table.Columns;
		var data = query.Data;
		var offset = query.Offset;

		// Bind header (9) + Param Formats (columns * 2) + Param Count (2) + Result Formats (4) + Execute (10) + Sync (5) = 30 + (columns * 6)
		var totalSize = 30 + (columns.Length * 6);

		foreach (ref readonly var col in columns)
		{
			var rawValue = data[col.RecordIndex + offset];
			if (rawValue is null) continue;

			if (col.FieldType == FieldType.ByteArray)
			{
				var len = rawValue.Length;
				if (len > 0)
				{
					var padding = rawValue.EndsWith("==", StringComparison.Ordinal) ? 2
								: rawValue.EndsWith('=') ? 1
								: 0;
					totalSize += (len * 3 / 4) - padding;
				}
			}
			else if (col.BinaryType)
			{
				totalSize += col.BinaryLength;
			}
			else
			{
				totalSize += encoding.GetByteCount(rawValue);
			}
		}

		return totalSize;
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static int WriteVariablesPayload(this in SaveQuery query, Span<byte> span, Encoding encoding)
	{
		ReadOnlySpan<Column> columns = query.Table.Columns;
		var data = query.Data;
		var offset = query.Offset;
		var paramCount = (short)columns.Length;

		// 1. Bind Header ('B')
		var bindStartOffset = 0;
		span[bindStartOffset] = (byte)FrontendMessageCode.Bind;
		span[5] = 0; // unnamed portal
		span[6] = 0; // unnamed statement

		// Number of parameter format codes specified
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(7, 2), paramCount);

		var writeOffset = 9;

		// Write parameter format codes (1 per column: 1 for binary, 0 for text)
		foreach (ref readonly var col in columns)
		{
			BinaryPrimitives.WriteInt16BigEndian(span.Slice(writeOffset, 2), col.BinaryType ? (short)1 : (short)0);
			writeOffset += 2;
		}

		// Write total parameter count
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(writeOffset, 2), paramCount);
		writeOffset += 2;

		// 2. Parameter Values Encoding
		foreach (ref readonly var col in columns)
		{
			var rawValue = data[col.RecordIndex + offset];

			if (rawValue is null)
			{
				BinaryPrimitives.WriteInt32BigEndian(span.Slice(writeOffset, 4), -1); // SQL NULL
				writeOffset += 4;
				continue;
			}

			var lengthHeaderOffset = writeOffset;
			writeOffset += 4;
			var valueStartOffset = writeOffset;

			if (col.FieldType == FieldType.ByteArray)
			{
				if (Convert.TryFromBase64String(rawValue, span[writeOffset..], out var bytesWritten))
				{
					writeOffset += bytesWritten;
				}
			}
			else if (col.BinaryType)
			{
				var dest = span.Slice(writeOffset, col.BinaryLength);
				ReadOnlySpan<char> valSpan = rawValue.AsSpan();

				switch (col.FieldType)
				{
					case FieldType.Long:
						BinaryPrimitives.WriteInt64BigEndian(dest, long.Parse(valSpan, CultureInfo.InvariantCulture));
						break;
					case FieldType.Int:
						BinaryPrimitives.WriteInt32BigEndian(dest, int.Parse(valSpan, CultureInfo.InvariantCulture));
						break;
					case FieldType.Short:
					case FieldType.Byte:
						BinaryPrimitives.WriteInt16BigEndian(dest, short.Parse(valSpan, CultureInfo.InvariantCulture));
						break;
					case FieldType.Double:
						BinaryPrimitives.WriteInt64BigEndian(dest, BitConverter.DoubleToInt64Bits(double.Parse(valSpan, CultureInfo.InvariantCulture)));
						break;
					case FieldType.Float:
						BinaryPrimitives.WriteInt32BigEndian(dest, BitConverter.SingleToInt32Bits(float.Parse(valSpan, CultureInfo.InvariantCulture)));
						break;
					case FieldType.Boolean:
						dest[0] = valSpan.Equals(BooleanTrue, StringComparison.Ordinal) ? (byte)1 : (byte)0;
						break;
				}

				writeOffset += col.BinaryLength;
			}
			else
			{
				writeOffset += encoding.GetBytes(rawValue, span[writeOffset..]);
			}

			// Patch parameter value length header
			BinaryPrimitives.WriteInt32BigEndian(span.Slice(lengthHeaderOffset, 4), writeOffset - valueStartOffset);
		}

		// Result format codes (Requesting Binary for all results)
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(writeOffset, 2), 1); // 1 format code follows
		writeOffset += 2;
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(writeOffset, 2), 1); // 1 = Binary
		writeOffset += 2;

		// Patch Bind message total length: (writeOffset - 1) gives size of payload excluding 'B' byte
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(1, 4), writeOffset - 1);

		// 3. Execute ('E')
		span[writeOffset++] = (byte)FrontendMessageCode.Execute;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(writeOffset, 4), 9);
		writeOffset += 4;
		span[writeOffset++] = 0; // unnamed portal
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(writeOffset, 4), 0); // maxRows = 0
		writeOffset += 4;

		// 4. Sync ('S')
		span[writeOffset++] = (byte)FrontendMessageCode.Sync;
		BinaryPrimitives.WriteInt32BigEndian(span.Slice(writeOffset, 4), 4);

		return writeOffset + 4;
	}

}
