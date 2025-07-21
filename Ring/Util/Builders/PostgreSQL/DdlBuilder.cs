using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Text;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
	private readonly static Dictionary<FieldType, string> _dataType = new()
	{
		{ FieldType.String,        "varchar"},
		{ FieldType.LongString,    "text"},
		{ FieldType.Double,        "float8"},
		{ FieldType.Float,         "float4"},
		{ FieldType.Long,          "int8"},
		{ FieldType.Int,           "int4"},
		{ FieldType.Short,         "int2"},
		{ FieldType.Byte,          "int2"},
		{ FieldType.Boolean,       "bool"},
		{ FieldType.ShortDateTime, "date"},
		{ FieldType.ByteArray,     "bytea"},
		{ FieldType.DateTime,      "timestamp without time zone"},
		{ FieldType.LongDateTime,  "timestamp without time zone"}
	};

	public DdlBuilder() : base() { }

	public sealed override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
	protected sealed override string StringCollateInformation => @"COLLATE ""C""";
	protected sealed override string MtmPrefix => "@mtm_";
	protected sealed override string TimeZoneOffsetPrefix => "@tz_offset_";
	protected sealed override int VarcharMaxSize => 65535;

	public sealed override string Create(TableSpace tablespace) // Code size: 77 (0x4d)
		=> new StringBuilder()
			.Append(DdlCreate)
			.Append(DdlTableSpace)
			.Append(tablespace.PhysicalName)
			.Append(@" LOCATION ")
			.Append('\'')
			.Append(tablespace.FileName)
			.Append('\'').ToString();

	protected sealed override Dictionary<FieldType, string> DataType => _dataType;
	protected sealed override string SchemaSeparator => ".";
	protected sealed override string StartPhysicalNameDelimiter => "\"";
	protected sealed override string EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
	protected sealed override string TablePrefix => DefaultTablePrefix;
	protected sealed override string SearchableFieldPrefix => "s_";

	public sealed override string GetPhysicalName(EntityType entityType, string name) => base.GetPhysicalName(entityType, name);

    protected sealed override string GetPhysicalName(Constraint constraint)
    {
        // Code size: 197 (0xc5)
        var result = new StringBuilder(31); // constraint name max length(30)
        switch (constraint.Type)
        {
            //name:  pk_{table_name}
            case ConstraintType.PrimaryKey:
                // apply short version of prefix 'pk'
                var prefix = constraint.ToTable.Name.Length > 27 ? DefaultPrimaryKeyPrefix[^1..] : DefaultPrimaryKeyPrefix;
                if (constraint.ToTable.Name.StartsWith(SpecialEntityPrefix))
                    result.Append(string.Join(null, StartPhysicalNameDelimiter, prefix, constraint.ToTable.Name, EndPhysicalNameDelimiter));
                else result.Append(string.Join(null, prefix, constraint.ToTable.Name));
                break;
        }
        return result.ToString();
    }
}
