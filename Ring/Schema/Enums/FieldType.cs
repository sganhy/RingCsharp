using Ring.Schema.Attributes;

namespace Ring.Schema.Enums;

/// <summary>
/// 	values equal or above to 125 are reserved for testing; value should be defined as [0,125[
/// </summary>
[Range(0, 124)]
internal enum FieldType : byte
{
	Long = 0,
	Int = 1,
	Short = 2,
	Byte = 3,
	Float = 14,
	Double = 15,
	String = 16,
	Date = 17,
	DateTime = 18,
	DateTimeOffset = 19,
	ByteArray = 21,
	Boolean = 23,
	LongString = 27,
	// 125 & 126 reserved for unit tests
	Undefined = 127
}