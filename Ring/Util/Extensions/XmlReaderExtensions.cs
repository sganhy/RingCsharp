using System.Runtime.CompilerServices;
using System.Xml;

namespace Ring.Util.Extensions;

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

	internal static void LoadAttributes(this XmlReader reader, Dictionary<string,string> values, bool nameSpaceIncluded = true)
	{
		// Code size: 64 (0x40)
		// clear dictionnary first
		values.ClearValues(string.Empty);
		if (!reader.HasAttributes) return;
		reader.MoveToFirstAttribute();
		do
		{
			var attributeName = RemoveNameSpaceInfo(reader.Name).ToUpperInvariant();
			if (values.ContainsKey(attributeName)) values[attributeName] = reader.Value;
		}
		while (reader.MoveToNextAttribute());
	}

	#region private methods 

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
	#endregion 
}
