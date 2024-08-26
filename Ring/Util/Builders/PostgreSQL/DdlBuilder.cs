using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Text;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly static Dictionary<FieldType, string> _dataType = new()
    {
        { FieldType.String,        "varchar" },
        { FieldType.LongString,    "text"    },
        { FieldType.Double,        "float8"  },
        { FieldType.Float,         "float4"  },
        { FieldType.Long,          "int8"    },
        { FieldType.Int,           "int4"    },
        { FieldType.Short,         "int2"    },
        { FieldType.Byte,          "int2"    },
        { FieldType.Boolean,       "bool"    },
        { FieldType.ShortDateTime, "date"    },
        { FieldType.ByteArray,     "bytea"   },
        { FieldType.DateTime,      "timestamp without time zone" },
        { FieldType.LongDateTime,  "timestamp with time zone" }
    };

    public DdlBuilder() : base() { }

    public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
    protected override string StringCollateInformation => @"COLLATE ""C""";
    protected override string MtmPrefix => "@mtm_";
    protected override int VarcharMaxSize => 65535;

    public override string Create(TableSpace tablespace)
    {
        var result = new StringBuilder();
        result.Append(DdlCreate)
            .Append(DdlTableSpace)
            .Append(GetPhysicalName(tablespace))
            .Append(@" LOCATION ")
            .Append('\'')
            .Append(tablespace.FileName)
            .Append('\'');
        return result.ToString();
    }
    protected override Dictionary<FieldType, string> DataType => _dataType;
    protected override char SchemaSeparator => '.';
    //TODO move to base class
    protected override string GetPhysicalName(TableSpace tablespace) => tablespace.Name;
    protected override char StartPhysicalNameDelimiter => '\"';
    protected override char EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
    protected override string TablePrefix => DefaultTablePrefix;


}
