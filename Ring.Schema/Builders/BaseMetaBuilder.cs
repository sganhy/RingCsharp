using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static int GetId(XmlReader reader, string idAttribute)
	{
		// Code size: 20 (0x14)
		var id = GetAttributeValue(reader, idAttribute);
		if (!int.TryParse(id, out int currentTableId)) currentTableId = -1;
		return currentTableId;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static string GetAttributeValue(XmlReader reader, string attribute)
	{
		// Code size: 51 (0x33)
		if (reader.MoveToFirstAttribute())
		{
			do
				if (string.Equals(attribute, reader.Name, StringComparison.OrdinalIgnoreCase)) return reader.Value;
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return string.Empty;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static (RelationType, string?, string?, bool, bool, bool) GetRelationInfo(XmlReader reader, SchemaTemplateAttribute relationTypeAttribute, SchemaTemplateAttribute relationToTableAttribute,
		SchemaTemplateAttribute relationInverseAttribute, SchemaTemplateAttribute relationBaselineAttribute, SchemaTemplateAttribute relationNotNullAttribute, SchemaTemplateAttribute relationConstraintAttribute)
	{
		// RelationType type, int toTableId, string? inverseRelation, bool baseline, bool notNull, bool constraint
		// Code size: 262 (0x106)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var attributeNameType = relationTypeAttribute.Name;
		var attributeNameTo = relationToTableAttribute.Name;
		var attributeNameInverse = relationInverseAttribute.Name;
		var attributeNameBaseLine = relationBaselineAttribute.Name;
		var attributeNameNotNull = relationNotNullAttribute.Name;
		var attributeNameConstraint = relationConstraintAttribute.Name;

		var type = RelationType.Undefined;
		string? toTable = null;
		string? inverseRelation = null;
		var baseline = false;
		var notNull = false;
		var constraint = false;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				if (string.Equals(attributeNameType, attributeName, comparison)) type = relationTypeAttribute.GetRelationType(reader.Value);
				if (string.Equals(attributeNameTo, attributeName, comparison)) toTable = reader.Value;
				if (string.Equals(attributeNameInverse, attributeName, comparison)) inverseRelation = reader.Value;
				if (string.Equals(attributeNameBaseLine, attributeName, comparison) && relationBaselineAttribute.GetFlagValue(reader.Value) == true) baseline = true;
				if (string.Equals(attributeNameNotNull, attributeName, comparison) && relationNotNullAttribute.GetFlagValue(reader.Value) == true) notNull = true;
				if (string.Equals(attributeNameConstraint, attributeName, comparison) && relationConstraintAttribute.GetFlagValue(reader.Value) == true) constraint = true;
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (type, toTable, inverseRelation, baseline, notNull, constraint);
	}

	

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (FieldType, SearchableType, int, string?, bool, bool, bool) GetFieldInfo(XmlReader reader, SchemaTemplateAttribute sizeAttribute, SchemaTemplateAttribute defaultValueAttribute,
		SchemaTemplateAttribute fieldBaseLineAttribute, SchemaTemplateAttribute fieldNotNullAttribute, SchemaTemplateAttribute fieldMultiLangualeAttribute, SchemaTemplateAttribute fieldTypeAttribute, 
		SchemaTemplateAttribute fieldCaseSensitiveAttribute)
	{
		// Code size: 262 (0x106)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var size = 0;
		string? defaultValue = null;
		var attributeNameSize = sizeAttribute.Name;
		var attributeNameDefaultValue = defaultValueAttribute.Name;
		var attributeNameBaseLine = fieldBaseLineAttribute.Name;
		var attributeNameNotNull = fieldNotNullAttribute.Name;
		var attributeNameMultiLanguale = fieldMultiLangualeAttribute.Name;
		var attributeNameType = fieldTypeAttribute.Name;
		var attributeNameCaseSensitive = fieldCaseSensitiveAttribute.Name;
		var fieldType = FieldType.Undefined;
		var searchableType = SearchableType.None;
		var baseline = false;
		var notNull = false;
		var multiLangual = false;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				if (string.Equals(attributeNameSize, attributeName, comparison)) size = sizeAttribute.GetFieldSize(reader.Value);
				if (string.Equals(attributeNameDefaultValue, attributeName, comparison)) defaultValue = reader.Value;
				if (string.Equals(attributeNameBaseLine, attributeName, comparison) && fieldBaseLineAttribute.GetFlagValue(reader.Value) == true) baseline = true;
				if (string.Equals(attributeNameNotNull, attributeName, comparison) && fieldNotNullAttribute.GetFlagValue(reader.Value) == true) notNull = true;
				if (string.Equals(attributeNameMultiLanguale, attributeName, comparison) && fieldMultiLangualeAttribute.GetFlagValue(reader.Value) == true) multiLangual = true;
				if (string.Equals(attributeNameType, attributeName, comparison)) fieldType = fieldTypeAttribute.GetFieldType(reader.Value);
				if (string.Equals(attributeNameCaseSensitive, attributeName, comparison)) searchableType = fieldCaseSensitiveAttribute.GetSearchableType(reader.Value);
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (fieldType, searchableType, size, defaultValue, baseline, notNull, multiLangual);
	}


	protected async static ValueTask<(string, bool, bool, bool)> GetIndexInfoAsync(XmlReader reader, SchemaTemplateAttribute indexBaselineAttribute, SchemaTemplateAttribute indexUniqueAttribute, SchemaTemplateItem columnIndex,
		Dictionary<string, SchemaTemplateItem> tagDico)
	{
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var attributeNameBaseLine = indexBaselineAttribute.Name;
		var attributeNameUnique = indexUniqueAttribute.Name;
		var colNameAttribute = columnIndex?.GetAttribute(SchemaTemplateAttributeType.Name)?.Name;
		var columnList = new StringBuilder();
		var indexColumnDelimiter = Meta.GetIndexColumnDelimiter();
		var unique = false;
		var bitmap = false;
		var baseline = false;
		if (reader.MoveToFirstAttribute() && colNameAttribute is not null)
		{
			do
			{
				var attributeName = reader.Name;
				if (string.Equals(attributeNameBaseLine, attributeName, comparison) && indexBaselineAttribute.GetFlagValue(reader.Value) == true) baseline = true;
				if (string.Equals(attributeNameUnique, attributeName, comparison) && indexUniqueAttribute.GetFlagValue(reader.Value) == true) unique = true;
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
			while (await reader.ReadAsync().ConfigureAwait(false))
			{
				var elementName = reader.Name;
				if (!tagDico.TryGetValue(elementName, out var item)) continue;
				if (item.EntityType == EntityType.IndexColumn && reader.NodeType == XmlNodeType.Element)
				{
					var name = GetAttributeValue(reader, colNameAttribute);
					columnList.Append(name);
					columnList.Append(indexColumnDelimiter);
				}
				if (item.EntityType == EntityType.Index && reader.NodeType == XmlNodeType.EndElement) break;
			}
			if (columnList.Length > 0) --columnList.Length;
		}
		return (columnList.ToString(), unique, bitmap, baseline);
	}

	protected static (bool, bool, bool) GetTableInfo(XmlReader reader, SchemaTemplateAttribute tableReadonlyAttribute, SchemaTemplateAttribute tableBaselineAttribute, SchemaTemplateAttribute tableCachedAttribute)
	{
		// Code size: 184 (0xb8)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var attributeNameBaseline = tableBaselineAttribute.Name;
		var attributeNameReadonly = tableReadonlyAttribute.Name;
		var attributeNameCached = tableCachedAttribute.Name;
		var cachedTable = false;
		var readonlyTable = false;
		var baseline = false;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				if (string.Equals(attributeNameBaseline, attributeName, comparison) && tableBaselineAttribute.GetFlagValue(reader.Value) == true)
					baseline = true;
				if (string.Equals(attributeNameReadonly, attributeName, comparison) && tableReadonlyAttribute.GetFlagValue(reader.Value) == true)
					readonlyTable = true;
				if (string.Equals(attributeNameCached, attributeName, comparison) && tableCachedAttribute.GetFlagValue(reader.Value) == true)
					cachedTable = true;

			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (readonlyTable, baseline, cachedTable);
	}

	protected static Meta SetDescription(ref Meta meta, string description) // Code size: 55 (0x37) 
		=> new(meta.Id, meta.ObjectType, meta.ReferenceId, meta.DataType, meta.Flags, meta.Name, description, meta.Value, meta.Active);

	protected static SchemaTemplate GetTemplate(DocumentType documentType)
		=> documentType.GetSchemaTemplate() ?? DefaultTemplate;

}
