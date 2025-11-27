namespace Ring.Schema.Models;

internal readonly struct DocumentStats
{
	readonly internal int MetaCount;
	readonly internal int SchemaCount;
	readonly internal int TableCount;
	readonly internal int FieldCount;
	readonly internal int RelationCount;
	readonly internal int IndexCount;
	readonly internal int ErrorCount;
	readonly internal int TableSpaceCount;
	readonly internal int LineCount;

	internal DocumentStats(int schemaCount, int tableCount, int fieldCount, int relationCount, int indexCount, int wrongParentCount, int tableSpaceCount, int lineCount, int metaCount)
	{
		SchemaCount = schemaCount;
		TableCount = tableCount;
		FieldCount = fieldCount;
		RelationCount = relationCount;
		IndexCount = indexCount;
		ErrorCount = wrongParentCount;
		TableSpaceCount = tableSpaceCount;
		LineCount = lineCount;
		MetaCount = metaCount;
	}
}
