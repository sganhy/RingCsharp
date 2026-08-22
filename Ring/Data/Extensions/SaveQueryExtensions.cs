using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Util.Helpers;
using System.Text;

namespace Ring.Data.Extensions;

internal static class SaveQueryExtensions
{
	private const char HashCodeSeparator = (char)4444;

	internal static int GetHashCode(this SaveQuery saveQuery)
	{
		// Code size: 15 (0xf)
		var hash = 55;
		return hash;
	}

	internal static string GetStringCode(this SaveQuery saveQuery)
	{
        // Code size: 83 (0x53)
        /*
		*	readonly Table Table
		*	readonly SaveQueryType Type
		*	readonly IDmlBuilder Builder
		*	readonly string?[] Data
		*	readonly int Offset
		*/
        return new StringBuilder()
			.Append(saveQuery.Table.PhysicalName)
            .Append(HashCodeSeparator)
            .Append(saveQuery.Type.ToString())
            // ignore Builder
            .Append(HashCodeSeparator)
            .Append(saveQuery.Offset).ToString();
	}

}
