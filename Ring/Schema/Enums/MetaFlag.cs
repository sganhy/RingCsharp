namespace Ring.Schema.Enums;

[Flags]
internal enum MetaFlag : long
{
#pragma warning disable CA1069 // Enums values should not be duplicated

	None = 0,
	FieldNotNull = 1L << 2,          // bit 3
	FieldMultilingual = 1L << 3,     // bit 4
	RelationNotNull = 1L << 3,       // bit 4
	RelationConstraint = 1L << 4,    // bit 5
    TablePreparedStatment = 1L << 6, // bit 7
    TableSoftDelete = 1L << 7,       // bit 8
	IndexBitmap = 1L << 8,           // bit 9
	TableCached = 1L << 8,           // bit 9
	IndexUnique = 1L << 9,           // bit 10
	TableReadonly = 1L << 9,         // bit 10
	FieldAllowTruncation = 1L << 10, // bit 11
	TablespaceIndex = 1L << 10,      // bit 11
	TablespaceTable = 1L << 11,      // bit 12
	EntityBaseline = 1L << 13,       // bit 14

#pragma warning restore CA1069
}

