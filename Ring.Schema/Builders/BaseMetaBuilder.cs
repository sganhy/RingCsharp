using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

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

	// templates
	protected readonly static SchemaTemplate DefaultTemplate = new(string.Empty, DocumentType.Undefined, [], 0);
	protected readonly static Meta DefaultMetaField = new(0,FieldId,0,0,0L,string.Empty,null,null,true);
	protected readonly Dictionary<string, SchemaTemplateItem> TagDictionary;
	protected readonly SchemaTemplate Template;
	protected readonly DocumentType DocumentType;
	protected int LoadTemplateErrorCount;

	// attributes
    private readonly SchemaTemplateAttribute _tableIdAttribute; // TABLE
	private readonly SchemaTemplateAttribute _tableNameAttribute;
	private readonly SchemaTemplateAttribute _tableReadOnlyAttribute;
	private readonly SchemaTemplateAttribute _tableBaselineAttribute;
	private readonly SchemaTemplateAttribute _tableCachedAttribute;
	private readonly SchemaTemplateAttribute _fieldTypeAttribute; // FIELD
	private readonly SchemaTemplateAttribute _fieldNameAttribute;
	private readonly SchemaTemplateAttribute _fieldCaseSensitiveAttribute;
	private readonly SchemaTemplateAttribute _fieldSizeAttribute;
	private readonly SchemaTemplateAttribute _fieldDefaultValueAttribute;
	private readonly SchemaTemplateAttribute _fieldBaselineAttribute;
	private readonly SchemaTemplateAttribute _fieldNotNullAttribute;
	private readonly SchemaTemplateAttribute _fieldMultiLangualeAttribute;
	private readonly SchemaTemplateAttribute _relationNameAttribute; // RELATION
	private readonly SchemaTemplateAttribute _relationTypeAttribute;
	private readonly SchemaTemplateAttribute _relationToTableAttribute;
	private readonly SchemaTemplateAttribute _relationInverseAttribute;
	private readonly SchemaTemplateAttribute _relationBaselineAttribute;
	private readonly SchemaTemplateAttribute _relationNotNullAttribute;
	private readonly SchemaTemplateAttribute _relationConstraintAttribute;
	private readonly SchemaTemplateAttribute _indexNameAttribute; // INDEX
	private readonly SchemaTemplateAttribute _indexBaselineAttribute;
	private readonly SchemaTemplateAttribute _indexUniqueAttribute;
	private readonly SchemaTemplateAttribute _indexColumnAttribute; // INDEX COLUMN
	private readonly SchemaTemplateAttribute _schemaNamedAttribute; // SCHEMA
	private readonly SchemaTemplateAttribute _tablespaceNameAttribute; // TABLESPACE
	private readonly SchemaTemplateAttribute _tablespaceFileAttribute;
	private readonly SchemaTemplateAttribute _tablespaceTableAttribute;
	private readonly SchemaTemplateAttribute _tablespaceIndexAttribute;

	internal BaseMetaBuilder(SchemaTemplate template, Dictionary<string, SchemaTemplateItem> tagDico, DocumentType documentType)
	{
		Template = template;
		TagDictionary = tagDico;
		DocumentType = documentType;

		// load templates items
		LoadTemplateErrorCount = 0;

		// load attributes 
		_tableIdAttribute = template.GetAttribute(EntityType.Table, SchemaTemplateAttributeType.Id, ref LoadTemplateErrorCount);
		_tableNameAttribute = template.GetAttribute(EntityType.Table, SchemaTemplateAttributeType.Name, ref LoadTemplateErrorCount);
		_tableReadOnlyAttribute = template.GetAttribute(EntityType.Table, SchemaTemplateAttributeType.ReadOnly, ref LoadTemplateErrorCount);
		_tableBaselineAttribute = template.GetAttribute(EntityType.Table, SchemaTemplateAttributeType.BaseLine, ref LoadTemplateErrorCount);
		_tableCachedAttribute = template.GetAttribute(EntityType.Table, SchemaTemplateAttributeType.Cached, ref LoadTemplateErrorCount);
		_fieldTypeAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.Type, ref LoadTemplateErrorCount); // field 
		_fieldNameAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.Name, ref LoadTemplateErrorCount);
		_fieldCaseSensitiveAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.CaseSensitive, ref LoadTemplateErrorCount);
		_fieldSizeAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.Size, ref LoadTemplateErrorCount);
		_fieldDefaultValueAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.DefaultValue, ref LoadTemplateErrorCount);
		_fieldBaselineAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.BaseLine, ref LoadTemplateErrorCount);
		_fieldNotNullAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.NotNull, ref LoadTemplateErrorCount);
		_fieldMultiLangualeAttribute = template.GetAttribute(EntityType.Field, SchemaTemplateAttributeType.Multilingual, ref LoadTemplateErrorCount);
		_relationNameAttribute = template.GetAttribute(EntityType.Relation, SchemaTemplateAttributeType.Name, ref LoadTemplateErrorCount); // relation
		_relationTypeAttribute = template.GetAttribute(EntityType.Relation, SchemaTemplateAttributeType.Type, ref LoadTemplateErrorCount);
		_relationToTableAttribute = template.GetAttribute(EntityType.Relation, SchemaTemplateAttributeType.To, ref LoadTemplateErrorCount);
		_relationInverseAttribute = template.GetAttribute(EntityType.Relation, SchemaTemplateAttributeType.InverseRelation, ref LoadTemplateErrorCount);
		_relationBaselineAttribute = template.GetAttribute(EntityType.Relation, SchemaTemplateAttributeType.BaseLine, ref LoadTemplateErrorCount);
		_relationNotNullAttribute = template.GetAttribute(EntityType.Relation, SchemaTemplateAttributeType.NotNull, ref LoadTemplateErrorCount);
		_relationConstraintAttribute = template.GetAttribute(EntityType.Relation, SchemaTemplateAttributeType.Constraint, ref LoadTemplateErrorCount);
		_indexNameAttribute = template.GetAttribute(EntityType.Index, SchemaTemplateAttributeType.Name, ref LoadTemplateErrorCount); // index
		_indexBaselineAttribute = template.GetAttribute(EntityType.Index, SchemaTemplateAttributeType.BaseLine, ref LoadTemplateErrorCount);
		_indexUniqueAttribute = template.GetAttribute(EntityType.Index, SchemaTemplateAttributeType.Unique, ref LoadTemplateErrorCount);
		_indexColumnAttribute = template.GetAttribute(EntityType.IndexColumn, SchemaTemplateAttributeType.Name, ref LoadTemplateErrorCount);
		_schemaNamedAttribute = template.GetAttribute(EntityType.Schema, SchemaTemplateAttributeType.Name, ref LoadTemplateErrorCount);
		_tablespaceNameAttribute = template.GetAttribute(EntityType.Tablespace, SchemaTemplateAttributeType.Name, ref LoadTemplateErrorCount);
		_tablespaceFileAttribute = template.GetAttribute(EntityType.Tablespace, SchemaTemplateAttributeType.File, ref LoadTemplateErrorCount);
		_tablespaceTableAttribute = template.GetAttribute(EntityType.Tablespace, SchemaTemplateAttributeType.Table, ref LoadTemplateErrorCount);
		_tablespaceIndexAttribute = template.GetAttribute(EntityType.Tablespace, SchemaTemplateAttributeType.Index, ref LoadTemplateErrorCount);
	}

	#region mapper to Meta struct

	protected static Meta ToTablespace(string name, string file, bool table, bool index)
	{
		var flags = 0L;
		Meta.SetTablespaceTable(flags, table);
		Meta.SetTablespaceIndex(flags, index);
		return new(0, TableSpaceId, 0, 0, flags, name, string.Empty, string.Empty, true);
	}

	protected static Meta ToSchema(string name)
	{
		return new(0, SchemaId, 0, 0, 0L, name, string.Empty, null, true);
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

	#endregion 

	protected string GetSchemaInfo(XmlReader reader)
	{
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var attributeNameName = _schemaNamedAttribute.Name;
		var name = string.Empty;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				var attributeValue = reader.Value ?? string.Empty;
				if (string.Equals(attributeNameName, attributeName, comparison)) name = attributeValue;
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return name;
	}

	protected (string, string, bool , bool) GetTableSpaceInfo(XmlReader reader)
	{
		// Code size: 375 (0x177)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var attributeNameName = _tablespaceNameAttribute.Name;
		var attributeFileName = _tablespaceFileAttribute.Name;
		var attributeTableName = _tablespaceTableAttribute.Name;
		var attributeIndexName = _tablespaceIndexAttribute.Name;

		var table = false;
		var index = false;
		var name = string.Empty;
		var file = string.Empty;
	
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				var attributeValue = reader.Value ?? string.Empty;
				if (string.Equals(attributeNameName, attributeName, comparison)) name = attributeValue;
				if (string.Equals(attributeFileName, attributeName, comparison)) file = attributeValue;
				if (string.Equals(attributeTableName, attributeName, comparison) && _tablespaceTableAttribute.GetFlagValue(attributeValue) == true) table = true;
				if (string.Equals(attributeIndexName, attributeName, comparison) && _tablespaceIndexAttribute.GetFlagValue(attributeValue) == true) index = true;
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (name, file, table, index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected (string, RelationType, string?, string?, bool, bool, bool) GetRelationInfo(XmlReader reader)
	{
		// RelationType type, int toTableId, string? inverseRelation, bool baseline, bool notNull, bool constraint
		// Code size: 375 (0x177)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var attributeNameName = _relationNameAttribute.Name;
		var attributeNameType = _relationTypeAttribute.Name;
		var attributeNameTo = _relationToTableAttribute.Name;
		var attributeNameInverse = _relationInverseAttribute.Name;
		var attributeNameBaseLine = _relationBaselineAttribute.Name;
		var attributeNameNotNull = _relationNotNullAttribute.Name;
		var attributeNameConstraint = _relationConstraintAttribute.Name;

		var type = RelationType.Undefined;
		string? toTable = null;
		string? inverseRelation = null;
		var baseline = false;
		var notNull = false;
		var constraint = false;
		var name = string.Empty;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				var attributeValue = reader.Value ?? string.Empty;
				if (string.Equals(attributeNameType, attributeName, comparison)) type = _relationTypeAttribute.GetRelationType(attributeValue);
				if (string.Equals(attributeNameTo, attributeName, comparison)) toTable = attributeValue;
				if (string.Equals(attributeNameInverse, attributeName, comparison)) inverseRelation = attributeValue;
				if (string.Equals(attributeNameBaseLine, attributeName, comparison) && _relationBaselineAttribute.GetFlagValue(attributeValue) == true) baseline = true;
				if (string.Equals(attributeNameNotNull, attributeName, comparison) && _relationNotNullAttribute.GetFlagValue(attributeValue) == true) notNull = true;
				if (string.Equals(attributeNameConstraint, attributeName, comparison) && _relationConstraintAttribute.GetFlagValue(attributeValue) == true) constraint = true;
				if (string.Equals(attributeNameName, attributeName, comparison)) name = attributeValue;
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (name, type, toTable, inverseRelation, baseline, notNull, constraint);
	}

	

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected (string, FieldType, SearchableType, int, string?, bool, bool, bool) GetFieldInfo(XmlReader reader)
	{
		// Code size: 438 (0x1b6)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var size = 0;
		string? defaultValue = null;
		var attributeNameSize = _fieldSizeAttribute.Name;
		var attributeNameDefaultValue = _fieldDefaultValueAttribute.Name;
		var attributeNameBaseLine = _fieldBaselineAttribute.Name;
		var attributeNameNotNull = _fieldNotNullAttribute.Name;
		var attributeNameMultiLanguale = _fieldMultiLangualeAttribute.Name;
		var attributeNameType = _fieldTypeAttribute.Name;
		var attributeNameCaseSensitive = _fieldCaseSensitiveAttribute.Name;
		var attributeNameName = _fieldNameAttribute.Name;
		var fieldType = FieldType.Undefined;
		var searchableType = SearchableType.None;
		var baseline = false;
		var notNull = false;
		var multiLangual = false;
		var name = string.Empty;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				var attributeValue = reader.Value ?? string.Empty; 
				if (string.Equals(attributeNameSize, attributeName, comparison)) size = _fieldSizeAttribute.GetFieldSize(attributeValue);
				if (string.Equals(attributeNameDefaultValue, attributeName, comparison)) defaultValue = attributeValue;
				if (string.Equals(attributeNameBaseLine, attributeName, comparison) && _fieldBaselineAttribute.GetFlagValue(attributeValue) == true) baseline = true;
				if (string.Equals(attributeNameNotNull, attributeName, comparison) && _fieldNotNullAttribute.GetFlagValue(attributeValue) == true) notNull = true;
				if (string.Equals(attributeNameMultiLanguale, attributeName, comparison) && _fieldMultiLangualeAttribute.GetFlagValue(attributeValue) == true) multiLangual = true;
				if (string.Equals(attributeNameType, attributeName, comparison)) fieldType = _fieldTypeAttribute.GetFieldType(attributeValue);
				if (string.Equals(attributeNameCaseSensitive, attributeName, comparison)) searchableType = _fieldCaseSensitiveAttribute.GetSearchableType(attributeValue);
				if (string.Equals(attributeNameName, attributeName, comparison)) name = attributeValue;
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (name, fieldType, searchableType, size, defaultValue, baseline, notNull, multiLangual);
	}


	protected async ValueTask<(string, string, bool, bool, bool)> GetIndexInfoAsync(XmlReader reader, Dictionary<string, SchemaTemplateItem> tagDico)
	{
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var attributeNameBaseLine = _indexBaselineAttribute.Name;
		var attributeNameUnique = _indexUniqueAttribute.Name;
		var attributeNameName = _indexNameAttribute.Name;
		var colNameAttribute = _indexColumnAttribute.Name;
		var columnList = new StringBuilder();
		var indexColumnDelimiter = Meta.GetIndexColumnDelimiter();
		var indexName = string.Empty;
		var unique = false;
		var bitmap = false;
		var baseline = false;
		if (reader.MoveToFirstAttribute() && colNameAttribute is not null)
		{
			do
			{
				var attributeName = reader.Name;
				var attributeValue = reader.Value ?? string.Empty;
				if (string.Equals(attributeNameBaseLine, attributeName, comparison) && _indexBaselineAttribute.GetFlagValue(attributeValue) == true) baseline = true;
				if (string.Equals(attributeNameUnique, attributeName, comparison) && _indexUniqueAttribute.GetFlagValue(attributeValue) == true) unique = true;
				if (string.Equals(attributeNameName, attributeName, comparison)) indexName = attributeValue;
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
			while (await reader.ReadAsync().ConfigureAwait(false))
			{
				var elementName = reader.Name;
				if (!tagDico.TryGetValue(elementName, out var item)) continue;
				if (item.EntityType == EntityType.IndexColumn && reader.NodeType == XmlNodeType.Element)
				{
					var name = reader.GetAttributeValue(colNameAttribute);
					columnList.Append(name);
					columnList.Append(indexColumnDelimiter);
				}
				if (item.EntityType == EntityType.Index && reader.NodeType == XmlNodeType.EndElement) break;
			}
			if (columnList.Length > 0) --columnList.Length;
		}
		return (indexName, columnList.ToString(), unique, bitmap, baseline);
	}

	protected (int, string, bool, bool, bool) GetTableInfo(XmlReader reader)
	{
		// Code size: 306 (0x132)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var attributeNameReadonly = _tableReadOnlyAttribute.Name;
		var attributeNameCached = _tableCachedAttribute.Name;
		var attributeNameBaseline = _tableBaselineAttribute.Name;
		var attributeNameName = _tableNameAttribute.Name;
		var attributeIdName = _tableIdAttribute.Name;
		var id = 0;
		var name = string.Empty;
		var cachedTable = false;
		var readonlyTable = false;
		var baseline = false;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				var attributeValue = reader.Value ?? string.Empty;
				if (string.Equals(attributeNameBaseline, attributeName, comparison) && _tableBaselineAttribute.GetFlagValue(attributeValue) == true) baseline = true;
				if (string.Equals(attributeNameReadonly, attributeName, comparison) && _tableReadOnlyAttribute.GetFlagValue(attributeValue) == true) readonlyTable = true;
				if (string.Equals(attributeNameCached, attributeName, comparison) && _tableCachedAttribute.GetFlagValue(attributeValue) == true) cachedTable = true;
				if (string.Equals(attributeNameName, attributeName, comparison)) name = attributeValue;
				if (string.Equals(attributeIdName, attributeName, comparison)) id = _tableIdAttribute.GetInteger(attributeValue);
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (id, name, readonlyTable, baseline, cachedTable);
	}

	protected static Meta SetDescription(ref Meta meta, string description) // Code size: 55 (0x37) 
		=> new(meta.Id, meta.ObjectType, meta.ReferenceId, meta.DataType, meta.Flags, meta.Name, description, meta.Value, meta.Active);

	protected static SchemaTemplate GetTemplate(DocumentType documentType)
		=> documentType.GetSchemaTemplate() ?? DefaultTemplate;


}
