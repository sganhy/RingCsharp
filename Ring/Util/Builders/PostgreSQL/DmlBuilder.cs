using Ring.Schema.Enums;
using System.Globalization;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DmlBuilder : BaseDmlBuilder
{
	private static readonly string ShortDateTimeWrapper = @"to_date({0},'YYYY-MM-DD')";
    private static readonly string DateTimeWrapper = @"to_timestamp({0},'YYYY-MM-DD HH24:MI:SS.MS')";
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
	public override string VariableNameTemplate => ":p{0}";

	protected override string WrapVariable(string variable, FieldType fieldType)
	{
		if (fieldType == FieldType.ShortDateTime) return string.Format(DefaultCulture, ShortDateTimeWrapper, variable);
        else if (fieldType == FieldType.DateTime) return string.Format(DefaultCulture, DateTimeWrapper, variable);
        return variable;
	}
}
