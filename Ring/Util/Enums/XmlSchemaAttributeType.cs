using Ring.Schema.Enums;
using Ring.Util.Attributes;

namespace Ring.Util.Enums;

/// <summary>
/// 	values above 125 are reserved for unitesting; value should be defined as [0,125[ 
/// </summary>
internal enum XmlSchemaAttributeType : byte
{
	Id = 0,
	Name = 1,
	BaseLine = 2,
	ReadOnly = 3,
	Cached = 4,

	[AttributeValue(EntityType.Relation, 128, 128 + 16, FieldType.String)]
    [AttributeValue(EntityType.Field, 145, 145 + 127, FieldType.String)]
    Type = 5,

	Size = 6,
	CaseSensitive = 7,
	NotNull = 8,
	Multilingual = 9,
	To = 10,
	InverseRelation = 11,
	Constraint = 12,
	Unique = 13,
	Undefined = 127
}
