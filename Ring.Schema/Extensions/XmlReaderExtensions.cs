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
				if (string.Equals(attributeTypeName, reader.Name, comparison))
					fieldType = attributeType.GetFieldType(reader.Value);
				if (string.Equals(attributeSearchableName, reader.Name, comparison))
					searchableType = attributeSearchable.GetSearchableType(reader.Value);
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		return (fieldType, searchableType);
	}
}
