using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Ring.Schema.Builders;

internal abstract class BaseMetaBuilder
{
	// consts
	protected const byte SchemaId = (byte)EntityType.Schema;
	protected const byte TableId = (byte)EntityType.Table;
	protected const byte FieldId = (byte)EntityType.Field;
	protected const byte RelationId = (byte)EntityType.Relation;
	protected const byte IndexId = (byte)EntityType.Index;
	protected const byte TableSpaceId = (byte)EntityType.Tablespace;

	// template 
	protected readonly static SchemaTemplate DefaultTemplate = new(string.Empty, DocumentType.Undefined, [], 0);
	protected readonly Dictionary<string, SchemaTemplateItem> TagDictionary;
	protected readonly SchemaTemplate Template;
	protected readonly DocumentType DocumentType;

	internal BaseMetaBuilder(SchemaTemplate template, Dictionary<string, SchemaTemplateItem> tagDico, DocumentType documentType)
	{
		Template = template;
		TagDictionary = tagDico;
		DocumentType = documentType;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static string GetAttributeValue(XmlReader reader, string attribute)
	{
		// Code size: 51 (0x33)
		if (reader.MoveToFirstAttribute())
		{
			do if (string.Equals(attribute, reader.Name, StringComparison.OrdinalIgnoreCase)) return reader.Value;
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return string.Empty;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static int GetId(XmlReader reader, string idAttribute)
	{
		// Code size: 24 (0x18)
		var id = GetAttributeValue(reader, idAttribute);
        if (!int.TryParse(id, out int currentTableId)) currentTableId = int.MinValue;
        return currentTableId;
	}

	protected static Meta ToTable(int id, string name, string? description, string? subject, int schemaId, TableType tableType, bool baseline, bool softDeletion, bool readonlyTable, bool cached)
	{
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, baseline);
		flags = Meta.SetTableReadonly(flags, readonlyTable);
		flags = Meta.SetTableCached(flags, cached);
		flags = Meta.SetPhysicalDeletion(flags, !softDeletion);
		flags = Meta.SetTableAllowAttributeExtension(flags, false);
		return new(id, TableId,  schemaId, (int)tableType, flags, name, description, subject, true);
	}

}
