using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Globalization;
using System.Text;

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
    private static readonly string DateTimeFormat = CastPrefix + DateFormat + " HH24:MI:SS.US')";
    private static readonly CompositeFormat ShortDateFormatComposite = CompositeFormat.Parse(ShortDateFormat);
    private static readonly CompositeFormat DateTimeFormatComposite = CompositeFormat.Parse(DateTimeFormat);
    public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
    public DqlBuilder() : base() {}

    protected override string GetSelection(in Column column)
    {
        switch (column.FieldType)
        {
            case FieldType.Date: 
                return string.Format(CultureInfo.InvariantCulture, ShortDateFormatComposite, column.PhysicalName);
            case FieldType.DateTime: 
                return string.Format(CultureInfo.InvariantCulture, DateTimeFormatComposite, column.PhysicalName);
            default: return column.PhysicalName;
        }
    }
}
