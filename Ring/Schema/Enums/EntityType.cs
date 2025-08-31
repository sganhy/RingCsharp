using Ring.Schema.Attributes;

namespace Ring.Schema.Enums;

/// <summary>
/// 	values equal or above to 125 are reserved for testing; value should be defined as [0,125[
/// </summary>
[Range(0, 124)]
internal enum EntityType : byte
{
	Table = 0,
	Field = 1,
	Relation = 2,
	Index = 3,
	View = 4,
	Schema = 7,
	Sequence = 15,
	Language = 17,
	Tablespace = 18,
	Parameter = 23,
	Alias = 25,
	// specific column definition [71, 79]
	SearchableColumn = 71,
	TimeZoneColumn = 72,
	// not stored in @meta table
	Constraint = 101,
	// 125 & 126 reserved for unit tests
	Undefined = 127
}