using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal interface IColumn
{
	int Id { get; }
	string Name { get; }
	FieldType FieldType { get; }
	RelationType RelationType { get; }
    EntityType Type { get; }
}
