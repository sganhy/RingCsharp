using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Ring.Schema.Extensions;

internal static class XmlReaderExtensions
{
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
