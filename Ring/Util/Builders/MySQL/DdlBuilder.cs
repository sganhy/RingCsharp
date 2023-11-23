using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.MySQL;

internal class DdlBuilder : BaseDdlBuilder
{
    private const int VarcharMaxSize = 65535;
    private readonly static string StringCollageInformation = @"COLLATE ""C""";
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

    public override string Create(TableSpace tablespace)
    {
        throw new NotImplementedException();
    }

    public override DatabaseProvider Provider => DatabaseProvider.MySql;

    protected override string GetDataType(Field field) =>
        GetDataType(_dataType[field.Type], field.Type, field.Size, VarcharMaxSize,
            field.Type == FieldType.String || field.Type == FieldType.LongString ?
            StringCollageInformation : null);

    protected override string GetDataType(Relation relation)
    {
        throw new NotImplementedException();
    }

    public override string GetPhysicalName(Field field) => field.Name;

    public override string GetPhysicalName(Relation relation)
    {
        throw new NotImplementedException();
    }

    protected override string GetPhysicalName(TableSpace tablespace)
    {
        throw new NotImplementedException();
    }

    protected override string GetPhysicalName(Schema.Models.Schema schema)
    {
        throw new NotImplementedException();
    }

    public override string GetPhysicalName(Table table, Schema.Models.Schema schema)
    {
        throw new NotImplementedException();
    }
}
