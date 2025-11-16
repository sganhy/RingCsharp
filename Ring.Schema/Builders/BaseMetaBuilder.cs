using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders;

internal abstract class BaseMetaBuilder
{
	// consts
	protected const byte SchemaId = (byte)EntityType.Schema;
	protected const byte TableId = (byte)EntityType.Table;
	protected const byte FieldId = (byte)EntityType.Field;
	protected const byte SearchableColumnId = (byte)EntityType.SearchableColumn;
	protected const byte RelationId = (byte)EntityType.Relation;
	protected const byte IndexId = (byte)EntityType.Index;
	protected const byte TableSpaceId = (byte)EntityType.Tablespace;
	protected readonly static string AllParent= @"*";

	// template 1
	protected readonly static SchemaTemplateAttribute DefaultTemplateAttribute = new(string.Empty, SchemaTemplateAttributeType.Undefined, []);
	protected readonly static SchemaTemplate DefaultTemplate = new(string.Empty, DocumentType.Undefined, [], 0);
	protected readonly static Meta DefaultMetaField = new(0,FieldId,0,0,0L,string.Empty,null,null,true);
	protected readonly Dictionary<string, SchemaTemplateItem> TagDictionary;
	protected readonly SchemaTemplate Template;
	protected readonly DocumentType DocumentType;

	internal BaseMetaBuilder(SchemaTemplate template, Dictionary<string, SchemaTemplateItem> tagDico, DocumentType documentType)
	{
		Template = template;
		TagDictionary = tagDico;
		DocumentType = documentType;
	}
		
	protected static Meta ToTable(int id, string name, string? description, string? subject, int schemaId, TableType tableType, bool baseline, bool softDeletion, bool readonlyTable, bool cached)
	{
		// Code size: 67 (0x43)
		/*
		 * int id, string name, string? description, string? subject, string physicalName, TableType type, Relation[] relations,
		 * Field[] fields, Column[] columns, Index[] indexes, int schemaId, PhysicalType physicalType, int objectIndex, int recordSize, CacheId cacheId,
		 * bool baseline, bool active, bool cached, bool allowHardDeletion, bool readonlyTable, bool usePreparedStatement, bool allowAttributeExtension
		*/
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, baseline);
		flags = Meta.SetTableReadonly(flags, readonlyTable);
		flags = Meta.SetTableCached(flags, cached);
		flags = Meta.SetPhysicalDeletion(flags, !softDeletion);
		flags = Meta.SetTableAllowAttributeExtension(flags, false);
		return new(id, TableId,  schemaId, (int)tableType, flags, name, description, subject, true);
	}

	protected static Meta ToField(int id, string name, string? description, FieldType type, int size, string? defaultValue, SearchableType searchableType, int referenceId, bool baseline, bool notNull, 
		bool multilingual, bool allowTruncation)
	{
		// Code size: 82 (0x52)
		/*
		 * int id, string name, string? description, FieldType type, int size, string? defaultValue, SearchableType searchableType,
		 * bool baseline, bool notNull, bool multilingual, bool allowTruncation, bool active
		*/
		var dataType = Meta.SetFieldType(0, type);
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, baseline);
		flags = Meta.SetFieldNotNull(flags, notNull);
		flags = Meta.SetFieldMultilingual(flags, multilingual);
		flags = Meta.SetFieldAllowTruncation(flags, allowTruncation);
		flags = Meta.SetFieldSize(flags, size);
		flags = Meta.SetSearchableType(flags, searchableType);
		return new(id, FieldId, referenceId, dataType, flags, name, description, defaultValue, true);
	}

	protected static Meta ToSearchableColumn(int id, string name, FieldType type, int size, string? defaultValue, SearchableType searchableType, int referenceId, bool baseline, bool notNull)
	{
		// Code size: 64 (0x40)
		/*
		 * int id, string name, string? description, FieldType type, int size, string? defaultValue, SearchableType searchableType,
		 * bool baseline, bool notNull, bool multilingual, bool allowTruncation, bool active
		*/
		var dataType = Meta.SetFieldType(0, type);
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, baseline);
		flags = Meta.SetFieldNotNull(flags, notNull);
		flags = Meta.SetFieldSize(flags, size);
		flags = Meta.SetSearchableType(flags, searchableType);
		return new(id, SearchableColumnId, referenceId, dataType, flags, name, null, defaultValue, true);
	}

	protected static Meta ToRelation(int id, string name, RelationType type, int toTableId, int referenceId, string? inverseRelation, bool baseline, bool notNull, bool constraint)
	{
		// Code size: 55 (0x37)
		/*
		 * int id, string name, string? description, RelationType type, Table toObject, FieldType fieldType, 
		 * bool notnull, bool constraint, bool baseline, bool active
		*/
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, baseline);
		flags = Meta.SetRelationdNotNull(flags, notNull);
		flags = Meta.SetRelationConstraint(flags, constraint);
		flags = Meta.SetRelationType(flags, type);
		return new(id, RelationId, referenceId, toTableId, flags, name, null, inverseRelation, true);
	}

	protected static Meta ToIndex(int id, string name, string columnList, int referenceId, bool unique, bool bitmap, bool baseline)
	{
		// Code size: 45 (0x2d)
		/*
		 * int id, string name, string? description, Column[] columns, string columnList, bool unique, bool bitmap, bool active, bool baseline
		*/
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, baseline);
		flags = Meta.SetIndexUnique(flags, unique);
		flags = Meta.SetIndexBitmap(flags, bitmap);
		return new(id, IndexId, referenceId, 0, flags, name, null, columnList, true);
	}

	protected static Meta SetDescription(ref Meta meta, string description) // Code size: 55 (0x37) 
		=> new(meta.Id, meta.ObjectType, meta.ReferenceId, meta.DataType, meta.Flags, meta.Name, description, meta.Value, meta.Active);

	protected static SchemaTemplate GetTemplate(DocumentType documentType)
		=> documentType.GetSchemaTemplate() ?? DefaultTemplate;

}
