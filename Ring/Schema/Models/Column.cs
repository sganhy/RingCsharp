using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class Column : IEquatable<Column>
{
	internal readonly int Id;
	internal readonly EntityType Type;
	internal readonly FieldType FieldType;
	internal readonly SearchableType SearchableType;
	internal readonly string PhysicalName;
	internal readonly int RecordIndex;

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
	public bool Equals(Column? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Id} - {PhysicalName}";
#endif
}