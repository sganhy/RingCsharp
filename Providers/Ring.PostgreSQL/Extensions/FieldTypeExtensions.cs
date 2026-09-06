using Ring.Schema.Enums;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class FieldTypeExtensions
{
	/// <summary>
	///     Returns true for <see cref="FieldType"/> values that are encoded as binary
	///     (format code 1) in the Bind message. Must stay in sync with every case
	///     <see cref="GetBinaryParamLength"/> treats as fixed-width - a type present
	///     in one but not the other sizes the parameter for binary but writes it as
	///     text (or vice versa), corrupting the message. ByteArray is handled
	///     separately via <paramref name="byteaBytes"/> so is not included here.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsBinaryType(this FieldType fieldType) =>
		fieldType is FieldType.Long or FieldType.Int or FieldType.Short or FieldType.Byte or FieldType.Double or FieldType.Float or FieldType.Boolean;


	/// <summary>
	///     Returns the wire byte length for a parameter value given its <see cref="FieldType"/>.
	///     Numeric types use fixed binary sizes; everything else uses UTF-8 byte count.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetBinaryParamLength(this FieldType fieldType, string value, Encoding encoding) =>
		fieldType switch
		{
			FieldType.Long => 8,
			FieldType.Int => 4,
			FieldType.Short => 2,
			FieldType.Byte => 2,
			FieldType.Double => 8,
			FieldType.Float => 4,
			FieldType.Boolean => 1,
			_ => encoding.GetByteCount(value)
		};

}
