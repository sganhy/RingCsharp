using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Column
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

#if DEBUG
	public override string ToString() => $"{Id} - {PhysicalName}";
#endif


}