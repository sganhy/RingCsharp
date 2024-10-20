namespace Ring.Data;

// Enums should have zero value
// Mark enums with FlagsAttribute
// Enum Storage should be Int32
#pragma warning disable CA1008, CA1027, CA1028
public enum SortOrder : byte
{
#pragma warning restore CA1028, CA1027, CA1008
	Descending = 1,
	Ascending = 2
}
