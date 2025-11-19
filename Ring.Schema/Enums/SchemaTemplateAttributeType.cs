namespace Ring.Schema.Enums;

internal enum SchemaTemplateAttributeType : short
{
	Id = 0,
	Name = 1,
	BaseLine = 2,
	ReadOnly = 3,
	Cached = 4,
	Type = 5,
	Size = 6,
	CaseSensitive = 7,
	NotNull = 8,
	Multilingual = 9,
	To = 10,
	InverseRelation = 11,
	Constraint = 12,
	Unique = 13,
	Parent = 22,
	Value = 23,
	Val = 24, 
	Attribute = 25,
	DefaultValue = 26,
	Template = 29,
	Depth = 31,
	Table = 101,
	Index = 102,
	File = 103,
	Undefined = short.MaxValue
}
