using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal readonly struct Column : IEquatable<Column>
{
	// Total: Class: 40 bytes per instance. Struct:	24 bytes per instance
	// padding (object size must round up to 8-byte multiple) size equal to 5
	// As a struct array, the 30–40 columns of a table sit in one contiguous block, so iterating them is a linear memory scan the CPU prefetcher handles efficiently.
	internal readonly int Id;
	internal readonly int RecordIndex;
	internal readonly EntityType Type; // enum EntityType : byte
	internal readonly FieldType FieldType; // enum FieldType : byte
	internal readonly SearchableType SearchableType; // enum SearchableType : byte
	internal readonly string PhysicalName;

	internal Column(EntityType entityType, FieldType fieldType, string physicalName, SearchableType searchableType, int id, int recordIndex)
	{
		Id = id;
		FieldType = fieldType;
		PhysicalName = physicalName;
		Type = entityType;
		RecordIndex = recordIndex;
		SearchableType = searchableType;
	}

	public static bool operator ==(Column left, Column right) => left.Equals(right);
	public static bool operator !=(Column left, Column right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is Column column && Equals(column);
	public bool Equals(Column other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Id} - {PhysicalName}";
#endif
}