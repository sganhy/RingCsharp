using Ring.Schema.Attributes;

namespace Ring.Schema.Enums;

/// <summary>
/// 	values above 15 are reserved for testing; value should be defined as [1,16[
/// </summary>
[Range(1, 15)]
internal enum RelationType : byte
{
	Otop = 1,
	Otm = 2,
	Mtm = 3,
	Mto = 11,
	Otof = 12,
	// 16 reserved for unit tests
	Undefined = 16
}