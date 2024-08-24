using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.Static.SQLServer;

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
        { FieldType.ShortDateTime, "date"           },
        { FieldType.ByteArray,     "varbinary(MAX)" },
        { FieldType.DateTime,      "datetime2"      },
        { FieldType.LongDateTime,  "datetimeoffset" }
    };

    public DdlBuilder() : base() { }

    public override string Create(TableSpace tablespace)
    {
        throw new NotImplementedException();
    }
    protected override string GetPhysicalName(TableSpace tablespace)
    {
        throw new NotImplementedException();
    }
    public override DatabaseProvider Provider => DatabaseProvider.SqlServer;
    protected override string MtmPrefix => "@mtm_";
    protected override Dictionary<FieldType, string> DataType => _dataType;
    protected override int VarcharMaxSize => -1;
    protected override string StringCollateInformation => throw new NotImplementedException();
    protected override char SchemaSeparator => '.';
    protected override char StartPhysicalNameDelimiter => '[';
    protected override char EndPhysicalNameDelimiter => ']';
    protected override string TablePrefix => DefaultTablePrefix;

}
