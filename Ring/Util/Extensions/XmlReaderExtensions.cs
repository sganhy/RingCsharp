using System.Xml;

namespace Ring.Util.Extensions;

internal static class XmlReaderExtensions
{

	/// <summary>
	/// 	Get Attribute value by key name  (case insensitive) 
	/// </summary>
	internal static void LoadAttributes(this XmlReader reader, Dictionary<string,string> values, bool nameSpaceIncluded = true)
	{
        // Code size: 64 (0x40)
        if (!reader.HasAttributes) return;
		reader.MoveToFirstAttribute();
		do
		{
			var attributeName = RemoveNameSpaceInfo(reader.Name).ToUpperInvariant();
			if (values.ContainsKey(attributeName)) values[attributeName] = reader.Value;
		}
		while (reader.MoveToNextAttribute());
	}

	/// <summary>
	/// 	Remove namespace information
	/// </summary>
	private static string RemoveNameSpaceInfo(string attributeName)
	{
		// Code size: 28 (0x1c)
		if (attributeName is null) return null;
		var index = attributeName.LastIndexOf(':');
		return index >= 0 ? attributeName.Substring(index) : attributeName;
	}

}
