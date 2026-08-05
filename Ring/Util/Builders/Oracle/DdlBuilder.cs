using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring.Util.Builders.Oracle;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly static DatabaseProvider _currentProvider = DatabaseProvider.Oracle;
    private readonly static Dictionary<FieldType, string> _dataType = new()
    {
        { FieldType.String,        "VARCHAR"   },
        { FieldType.LongString,    "LONGTEXT"  },
        { FieldType.Double,        "DOUBLE"    },
        { FieldType.Float,         "FLOAT"     },
        { FieldType.Long,          "BIGINT"    },
        { FieldType.Int,           "INT"       },
        { FieldType.Short,         "SMALLINT"  },
        { FieldType.Byte,          "TINYINT"   },
        { FieldType.Boolean,       "BOOLEAN"   },
        { FieldType.Date, "DATE"      },
        { FieldType.ByteArray,     "VARBINARY" },
        { FieldType.DateTime,      "TIMESTAMP" },
        { FieldType.DateTimeOffset,  "TIMESTAMP" }
    };

    public DdlBuilder() : base() { }

    public sealed override string Create(TableSpace tablespace) => tablespace.Name;
    public sealed override DatabaseProvider Provider => _currentProvider;
    protected sealed override string MtmPrefix => TableType.Mtm.GetLogicalName(); // physical name prefix for many-to-many tables
	protected sealed override string? TimeZoneOffsetPrefix => null;
    protected sealed override Dictionary<FieldType, string> DataType => _dataType;
    protected sealed override int VarcharMaxSize => 65535;
    protected sealed override string StringCollateInformation => throw new NotImplementedException();
    protected sealed override string SchemaSeparator => ".";
	protected sealed override char PhysSpecialEntityPrefix => TableType.NonBusinessTable.GetLogicalName()[0];
	protected sealed override string StartPhysicalNameDelimiter => "`";
    protected sealed override string EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
    protected sealed override string TablePrefix => DefaultTablePrefix;
    protected sealed override string AlterColumnStatment => string.Empty;
    protected sealed override string SearchableFieldPrefix => "s_";
	protected override string GetCatalogPhysicalName(TableType tableType)
	{
		throw new NotImplementedException();
	}
	protected override string GetSchemaPhysicalName(TableType tableType)
	{
		throw new NotImplementedException();
	}
	protected override string GetPhysicalName(TableType tableType, Field field)
	{
		throw new NotImplementedException();
	}
	protected override Constraint? HasCheckConstraint(Table table, Column column) => null;

}
