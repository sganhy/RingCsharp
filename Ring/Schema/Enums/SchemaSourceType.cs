namespace Ring.Schema.Enums;

#pragma warning disable CA1027 // Mark enums with FlagsAttribute
public enum SchemaSourceType
#pragma warning restore CA1027 // Mark enums with FlagsAttribute
{
	UnDefined = 0,
	NativeXml = 1,
	ClfyXml = 2,
	NativeDataBase = 4,
	ClfyDataBase = 5,
}