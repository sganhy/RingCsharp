using Ring.Schema.Enums;
using System.Globalization;
using System.Text;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DmlBuilder : BaseDmlBuilder
{
	private static readonly string DateTemplate = "YYYY-MM-DD";   // must stay first
	private static readonly CompositeFormat ShortDateTimeWrapper = CompositeFormat.Parse("to_date({0},'" + DateTemplate + "')");
	private static readonly CompositeFormat DateTimeWrapper = CompositeFormat.Parse("to_timestamp({0},'" + DateTemplate + " HH24:MI:SS.US')");
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

	protected override string VariableNameTemplate => "${0}";
	internal DmlBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.PostgreSql, clientEncoding, logSql) { }

	protected override string WrapVariable(string variable, FieldType fieldType)
	{
		// Code size: 51 (0x33) - no virtual calls
		if (fieldType == FieldType.Date) return string.Format(DefaultCulture, ShortDateTimeWrapper, variable);
		if (fieldType == FieldType.DateTime || fieldType == FieldType.DateTimeOffset) return string.Format(DefaultCulture, DateTimeWrapper, variable);
		return variable;
	}
}
