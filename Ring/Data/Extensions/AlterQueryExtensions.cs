using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Enums;

namespace Ring.Data.Extensions;

internal static class AlterQueryExtensions
{
    internal static string? ToSql(this AlterQuery query)
    {
        var builder = query.Builder;
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (query.Type)
        {
            case AlterQueryType.CreateTable: return builder.Create(query.Table);
            case AlterQueryType.CreatePrimaryKey: return builder.Create(query.Constraint!);
        }
#pragma warning restore IDE0066
        return null;
    }


}
