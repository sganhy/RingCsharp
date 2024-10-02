namespace Ring.Schema.Enums;

internal enum TableType : byte
{
	Business = 1,
    BusinessLog = 2,
    Meta = 13,
	MetaId = 14,
	Fake = 15,
	Mtm = 16,
	Log = 17,
	Lexicon = 18,
	LexiconItem = 19,
	SchemaCatalog = 32,
	TableCatalog = 34,
	TableSpaceCatalog = 35,
	Logical = 37,
	Undefined = 127
}