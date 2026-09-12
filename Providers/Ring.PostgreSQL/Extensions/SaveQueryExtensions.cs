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
	private static readonly string PostGreTrue = "t";

	/// <summary>
	/// Calculates the exact byte size required for the Bind ('B') + Execute ('E') + Sync ('S') payload.
	/// Direct iteration with minimal IL overhead.
	/// </summary>
	internal static int GetVariablesPayloadSize(this in SaveQuery query, Encoding encoding)
	{
		// Code size: 228 (0xe4)
		ReadOnlySpan<Column> columns = query.Table.Columns;
		var data = query.Data;
		var offset = query.Offset;
		var totalSize = 28 + (columns.Length * 6);

		foreach (ref readonly var col in columns)
		{
			var rawValue = data[col.RecordIndex + offset];
			if (rawValue is null) continue;

			if (col.FieldType == FieldType.ByteArray)
			{
				var len = rawValue.Length;
				if (len > 0)
				{
					var padding = rawValue[^1] == '=' ? (len > 1 && rawValue[^2] == '=' ? 2 : 1) : 0;
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

	/// <summary>
	/// Writes the combined Bind ('B') + Execute ('E') + Sync ('S') payload directly to a destination memory span.
	/// Inlines binary value writing using direct Span parsing for minimal IL code size and execution complexity.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static int WriteVariablesPayload(this in SaveQuery query, Span<byte> span, Encoding encoding)
	{
		// Code size: 842 (0x34a) - removed boxing
		ReadOnlySpan<Column> columns = query.Table.Columns;
		var data = query.Data;
		var offset = query.Offset;
		var paramCount = (short)columns.Length;

		// 1. Bind Header ('B')
		span[0] = (byte)FrontendMessageCode.Bind;
		span[5] = 0; // unnamed portal
		span[6] = 0; // unnamed statement
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(7, 2), paramCount);

		var writeOffset = 9;

		// Format codes (1 per parameter)
		foreach (ref readonly var col in columns)
		{
			BinaryPrimitives.WriteInt16BigEndian(span.Slice(writeOffset, 2), col.BinaryType ? (short)1 : (short)0);
			writeOffset += 2;
		}

		// Parameter count header
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
				var valSpan = rawValue.AsSpan();

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
						dest[0] = (byte)(valSpan.Equals(BooleanTrue, StringComparison.OrdinalIgnoreCase) || valSpan.Equals(PostGreTrue, StringComparison.OrdinalIgnoreCase) ? 1 : 0);
						break;
				}

				writeOffset += col.BinaryLength;
			}
			else
			{
				writeOffset += encoding.GetBytes(rawValue, span[writeOffset..]);
			}

			// Patch value length header
			BinaryPrimitives.WriteInt32BigEndian(span.Slice(lengthHeaderOffset, 4), writeOffset - valueStartOffset);
		}

		// Result format codes
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(writeOffset, 2), 1);
		writeOffset += 2;
		BinaryPrimitives.WriteInt16BigEndian(span.Slice(writeOffset, 2), 0); // Text format
		writeOffset += 2;

		// Patch Bind message total length
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
