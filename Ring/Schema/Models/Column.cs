using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Column
{
	internal readonly int Id;
	internal readonly FieldType FieldType;
	internal readonly EntityType Type;
	internal readonly string PhysicalName;
	internal readonly SearchableType SearchableType;
	internal readonly int RecordIndex;
	internal readonly int Size;

	internal Column(FieldType fieldType, EntityType type, string physicalName, SearchableType searchableType, int id, int recordIndex, int size)
	{
		Id = id;
		FieldType = fieldType;
		Type = type;
		PhysicalName = physicalName;
		RecordIndex = recordIndex;
		SearchableType = searchableType;
		Size = size;
	}
}