using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.MySQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly static DatabaseProvider _currentProvider = DatabaseProvider.MySql;
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
    protected sealed override string MtmPrefix => "@mtm_";
    protected sealed override string? TimeZoneOffsetPrefix => null;
    protected sealed override Dictionary<FieldType, string> DataType => _dataType;
    protected sealed override int VarcharMaxSize => 65535;
    protected sealed override string StringCollateInformation => throw new NotImplementedException();
    protected sealed override string SchemaSeparator => ".";
    protected sealed override string StartPhysicalNameDelimiter => "`";
    protected sealed override string EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
    protected sealed override string TablePrefix => DefaultTablePrefix;
    protected sealed override string SearchableFieldPrefix => "s_";
    protected sealed override string GetPhysicalName(Constraint constraint) => string.Empty;
}
