using Ring.Data.Models;
using Ring.Util.Helpers;
using System.Text;

namespace Ring.Data.Extensions;

internal static class SaveQueryExtensions
{
    internal static int GetHashCode(this SaveQuery saveQuery)
    {
        HashHelper.Djb2X(saveQuery.GetStringCode(), out int hash);
        return hash;
    }

    internal static string GetStringCode(this SaveQuery saveQuery)
    {
        /*
         *   readonly Table Table
         *   readonly SaveQueryType Type
         *   readonly IDmlBuilder Builder
         *   readonly string?[] Data
         *   readonly int Offset
         */
        var result = new StringBuilder();
        result.Append(saveQuery.Table.PhysicalName);
        result.Append(saveQuery.Type.ToString());
        // ignore Builder
        result.Append(saveQuery.Offset);
        return result.ToString();
    }
}
