using Ring.Schema.Enums;
using Ring.Schema.Models;
using PostGreDdl = Ring.Util.Builders.PostgreSQL.DdlBuilder;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders.MySQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly static DatabaseProvider _currentProvider = DatabaseProvider.MySql;
    private readonly static PostGreDdl _postGreDdl = new();
    private readonly static string PhysicalNameSeparator = "`";
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
        { FieldType.ShortDateTime, "DATE"      },
        { FieldType.ByteArray,     "VARBINARY" },
        { FieldType.DateTime,      "TIMESTAMP" },
        { FieldType.LongDateTime,  "TIMESTAMP" }
    };

    public override string Create(TableSpace tablespace) => tablespace.Name;
    public override DatabaseProvider Provider => _currentProvider;
    protected override string MtmPrefix => "@mtm_";
    protected override Dictionary<FieldType, string> DataType => _dataType;
    protected override int VarcharMaxSize => 65535;
    protected override string StringCollateInformation => throw new NotImplementedException();

    protected override string GetPhysicalName(TableSpace tablespace) => tablespace.Name;

    public override string GetPhysicalName(DbSchema schema) =>
        _postGreDdl.GetPhysicalName(schema).Replace(DefaultPhysicalNameSeparator, PhysicalNameSeparator);

    public override string GetPhysicalName(Table table, DbSchema schema) => 
        _postGreDdl.GetPhysicalName(table, schema).Replace(DefaultPhysicalNameSeparator, PhysicalNameSeparator);

}
