using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal interface IColumn
{
    int Id { get; }
    string Name { get; }
    FieldType Type { get; }
    RelationType RelationType { get; }

}
