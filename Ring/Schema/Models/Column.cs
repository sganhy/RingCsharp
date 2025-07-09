using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Column
{
	internal readonly int Id;
	internal readonly FieldType FieldType;
	internal readonly string PhysicalName;
	internal readonly SearchableType SearchableType;
	internal readonly int RecordIndex;

	internal Column(FieldType fieldType, string physicalName, SearchableType searchableType, int id, int recordIndex)
	{
		Id = id;
		FieldType = fieldType;
		PhysicalName = physicalName;
		RecordIndex = recordIndex;
		SearchableType = searchableType;
	}

#if DEBUG
	public override string ToString() => $"{Id} - {PhysicalName}";
#endif


}