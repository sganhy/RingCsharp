using Microsoft.VisualBasic;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Ring.Schema.Extensions;

internal static class XmlReaderExtensions
{

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string GetAttributeValue(this XmlReader reader, string attribute)
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
	internal static int GetId(this XmlReader reader, string idAttribute)
	{
		// Code size: 20 (0x14)
		var id = GetAttributeValue(reader, idAttribute);
		if (!int.TryParse(id, out int currentTableId)) currentTableId = -1;
		return currentTableId;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (FieldType, SearchableType) GetFieldInfo(this XmlReader reader, SchemaTemplateAttribute attributeType, SchemaTemplateAttribute attributeSearchable)
	{
		// Code size: 106 (0x6a)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var fieldType = FieldType.Undefined;
		var searchableType = SearchableType.None;
		var attributeTypeName = attributeType.Name;
		var attributeSearchableName = attributeSearchable.Name;
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				var attributeName = reader.Name;
				if (string.Equals(attributeTypeName, attributeName, comparison))
					fieldType = attributeType.GetFieldType(reader.Value);
				if (string.Equals(attributeSearchableName, attributeName, comparison))
					searchableType = attributeSearchable.GetSearchableType(reader.Value);
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (fieldType, searchableType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (int, string?, bool, bool, bool) GetFieldInfo(this XmlReader reader, SchemaTemplateAttribute sizeAttribute, SchemaTemplateAttribute defaultValueAttribute, 
		SchemaTemplateAttribute fieldBaseLineAttribute, SchemaTemplateAttribute fieldNotNullAttribute, SchemaTemplateAttribute fieldMultiLangualeAttribute)
	{
		// Code size: 262 (0x106)
		const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
		var size=0;
		string? defaultValue = null;
		var attributeNameSize = sizeAttribute.Name;
		var attributeNameDefaultValue = defaultValueAttribute.Name;
		var attributeNameBaseLine = fieldBaseLineAttribute.Name;
		var attributeNameNotNull = fieldNotNullAttribute.Name;
		var attributeNameMultiLanguale = fieldMultiLangualeAttribute.Name;
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
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (size, defaultValue, baseline, notNull, multiLangual);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (RelationType, string?, string?, bool, bool, bool) GetRelationInfo(this XmlReader reader, SchemaTemplateAttribute relationTypeAttribute, SchemaTemplateAttribute relationToTableAttribute,
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

	internal static (string, bool, bool, bool) GetIndexInfo(this XmlReader reader)
	{
		var columnList = string.Empty;
		var unique = false;
		var bitmap = false;
		var baseline = false;

		return (columnList, unique, bitmap, baseline);
	}

	internal static (bool, bool, bool) GetTableInfo(this XmlReader reader, SchemaTemplateAttribute tableReadonlyAttribute, SchemaTemplateAttribute tableBaselineAttribute, SchemaTemplateAttribute tableCachedAttribute)
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
}
