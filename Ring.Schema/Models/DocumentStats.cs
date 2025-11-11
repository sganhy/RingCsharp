namespace Ring.Schema.Models;

internal readonly struct DocumentStats
{
	readonly internal int SchemaCount;
	readonly internal int TableCount;
	readonly internal int FieldCount;
	readonly internal int UndefinedFieldTypeCount;
	readonly internal int RelationCount;
	readonly internal int IndexCount;
	readonly internal int WrongParentCount;
	readonly internal int TableSpaceCount;
	readonly internal int LineCount;

	internal DocumentStats(int schemaCount, int tableCount, int fieldCount, int undefinedFieldTypeCount, int relationCount, int indexCount, int wrongParentCount, int tableSpaceCount, int lineCount)
	{
		SchemaCount = schemaCount;
		TableCount = tableCount;
		FieldCount = fieldCount;
		UndefinedFieldTypeCount = undefinedFieldTypeCount;
		RelationCount = relationCount;
		IndexCount = indexCount;
		WrongParentCount = wrongParentCount;
		TableSpaceCount = tableSpaceCount;
		LineCount = lineCount;
	}
}
