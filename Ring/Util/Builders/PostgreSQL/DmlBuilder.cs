using Ring.Schema.Enums;
using System.Globalization;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DmlBuilder : BaseDmlBuilder
{
    private static readonly string DateTemplate = "YYYY-MM-DD";
    private static readonly string ShortDateTimeWrapper = "to_date({0},'"+ DateTemplate + "')";
    private static readonly string DateTimeWrapper = "to_timestamp({0},'" + DateTemplate + " HH24:MI:SS.US')";
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
	public override string VariableNameTemplate => ":p{0}";

	protected override string WrapVariable(string variable, FieldType fieldType)
	{
		if (fieldType == FieldType.ShortDateTime) return string.Format(DefaultCulture, ShortDateTimeWrapper, variable);
        else if (fieldType == FieldType.DateTime || fieldType == FieldType.LongDateTime) return string.Format(DefaultCulture, DateTimeWrapper, variable);
        return variable;
	}
}
