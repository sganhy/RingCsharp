using Ring.Schema.Attributes;

namespace Ring.Schema.Enums;

/// <summary>
/// 	values above 15 are reserved for testing; value should be defined as [1,127[
/// </summary>
[Range(1, 126)]
internal enum RelationType : byte
{
	Otop = 1,
	Otm = 2,
	Mtm = 3,
	Mto = 11,
	Otof = 12,
	// 127 reserved for unit tests
	Undefined = 127
}