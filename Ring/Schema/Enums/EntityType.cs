namespace Ring.Schema.Enums;

/// <summary>
/// 	values above 125 are reserved for unitesting; value should be defined as [0,125[ 
/// </summary>
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
	// columns definition
	SearchableColumn = 61,
	TimeZoneColumn = 62,
	// not stored in @meta table
	Constraint = 101,
	// 125 reserved for unit tests
	Undefined = 127
}