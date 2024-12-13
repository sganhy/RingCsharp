using Ring.Schema.Enums;
using System.Globalization;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DmlBuilder : BaseDmlBuilder
{
	private static readonly string ShortDateTimeWrapper = @"to_date({0},'YYYY-MM-DD')";
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
	public override string VariableNameTemplate => ":p{0}";

	protected override string WrapVariable(string variable, FieldType fieldType, int clauseId)
	{
		switch (clauseId)
		{
			case InsertClauseId:
				if (fieldType == FieldType.ShortDateTime)
					return string.Format(DefaultCulture, ShortDateTimeWrapper, variable);
				break;
		}
		return variable;
	}
}
