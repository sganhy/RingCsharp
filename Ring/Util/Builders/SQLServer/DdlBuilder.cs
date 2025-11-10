using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.SQLServer;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly static Dictionary<FieldType, string> _dataType = new()
    {
        { FieldType.String,        "varchar"        },
        { FieldType.LongString,    "bigtext"        },
        { FieldType.Double,        "float(53)"      },
        { FieldType.Float,         "real"           },
        { FieldType.Long,          "bigint"         },
        { FieldType.Int,           "int"            },
        { FieldType.Short,         "smallint"       },
        { FieldType.Byte,          "tinyint"        },   // [0, 255]
        { FieldType.Boolean,       "bit"            },
        { FieldType.Date, "date"           },
        { FieldType.ByteArray,     "varbinary(MAX)" },
        { FieldType.DateTime,      "datetime2"      },
        { FieldType.DateTimeOffset,  "datetimeoffset" }
    };

    public DdlBuilder() : base() { }

    public sealed override string Create(TableSpace tablespace)
    {
        throw new NotImplementedException();
    }
    public sealed override DatabaseProvider Provider => DatabaseProvider.SqlServer;
    protected sealed override string MtmPrefix => "@mtm_";
    protected sealed override string? TimeZoneOffsetPrefix => null;
    protected sealed override Dictionary<FieldType, string> DataType => _dataType;
    protected sealed override int VarcharMaxSize => -1;
    protected sealed override string StringCollateInformation => throw new NotImplementedException();
    protected sealed override string SchemaSeparator => ".";
    protected sealed override string StartPhysicalNameDelimiter => "[";
    protected sealed override string EndPhysicalNameDelimiter => "]";
    protected sealed override string TablePrefix => DefaultTablePrefix;
    protected sealed override string SearchableFieldPrefix => "s_";
    protected sealed override string GetPhysicalName(Constraint constraint) => string.Empty;
}
