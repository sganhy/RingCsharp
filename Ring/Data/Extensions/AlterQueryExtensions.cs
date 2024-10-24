using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Globalization;

namespace Ring.Data.Extensions;

internal static class AlterQueryExtensions
{
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

    internal static string? ToSql(this AlterQuery query)
    {
        var builder = query.Builder;
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (query.Type)
        {
            case AlterQueryType.CreateTable: return builder.Create(query.Table);
        }
#pragma warning restore IDE0066
        return null;
    }

    // error message for ddl exceptions
    internal static string ToErrorMessage(this AlterQuery query, Exception _)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (query.Type)
        {
            case AlterQueryType.CreateTable:
                return string.Format(DefaultCulture, ResourceHelper.GetErrorMessage(ResourceType.DdlException),
                    query.Table.PhysicalName);
        }
#pragma warning restore IDE0066

        return string.Empty;
    }

    internal static string ToLogUnsupportedOperation(this AlterQuery query)
    {
        // unsupported operation ? 
        if (ToSql(query) == null)
        {
            return string.Format(DefaultCulture, ResourceHelper.GetErrorMessage(ResourceType.UnsuportedOperation),
                    query.Type.ToString(), (int)query.Type);
        }
        return string.Empty;
    }

    internal static string ToLogOperationPerformed(this AlterQuery query, TimeSpan ts)
    {
        var displayMillisecond = Math.Max(ts.Milliseconds,1);

#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (query.Type)
        {
            case AlterQueryType.CreateTable:
                return string.Format(DefaultCulture, ResourceHelper.GetErrorMessage(ResourceType.DdlTableCreated),
                    query.Table.PhysicalName, displayMillisecond);
        }
#pragma warning restore IDE0066

        return string.Empty;
    }

}
