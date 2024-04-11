namespace Ring.Schema.Enums;

public enum SchemaLoadType 
{
	None = 0,
	Full = 1,
	QueryOnly = 2 // only fields, relations & unique indexes !
}