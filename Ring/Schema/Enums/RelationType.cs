namespace Ring.Schema.Enums;

internal enum RelationType : byte
{
	Otop = 1,
	Otm = 2,
	Mtm = 3,
	Mto = 11,
	Otof = 12,
	// 125 & 126 reserved for unit tests
	Undefined = 127
}