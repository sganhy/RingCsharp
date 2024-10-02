using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Globalization;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    /* TODO ==>
	 *   Float
	 *   Double 
	 *   LongDateTime
	 *   ByteArray 
	 */

    private static readonly string CastPrefix = "to_char({0},";
    private static readonly string DateFormat = "'yyyy-mm-dd";
    private static readonly string ShortDateFormat = CastPrefix + DateFormat + "')";
    private static readonly string DateTimeFormat = CastPrefix + DateFormat + "\"T\"HH24:MI:SS.US\"Z\"')";
    public sealed override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
    public DqlBuilder() : base() {}

    protected sealed override string GetSelection(Field field)
    {
        switch (field.Type)
        {
            case FieldType.ShortDateTime: 
                return string.Format(CultureInfo.InvariantCulture, ShortDateFormat, _ddlBuilder.GetPhysicalName(field));
            case FieldType.DateTime: 
                return string.Format(CultureInfo.InvariantCulture, DateTimeFormat, _ddlBuilder.GetPhysicalName(field));
            default: return _ddlBuilder.GetPhysicalName(field);
        }
    }
    protected sealed override string GetSelection(Relation relation) => _ddlBuilder.GetPhysicalName(relation);
}
