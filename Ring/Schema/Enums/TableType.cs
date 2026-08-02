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
	Test = 22,
	SchemaCatalog = 32,
	TableCatalog = 34,
	TablespaceCatalog = 35,
	Logical = 37,
	NonBusinessTable = 44, // non-business table, used to return logical system table prefixes. eg. @meta, @log, @mtm, @test, @lexicon, @lexicon_item
	// 124 reserved for unit tests
	Undefined = 127
}