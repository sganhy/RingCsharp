using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.SQLServer;

internal sealed class DdlBuilder : BaseDdlBuilder
{

    private const int VarcharMaxSize = -1; // no text size delimiter

    private readonly static Dictionary<FieldType, string> _dataType = new()
    {
        { FieldType.String,        "varchar"   },
        { FieldType.LongString,    "bigtext"   },
        { FieldType.Double,        "float(53)" },
        { FieldType.Float,         "real"      },
        { FieldType.Long,          "bigint"    },
        { FieldType.Int,           "int"       },
        { FieldType.Short,         "smallint"  },
        { FieldType.Byte,          "tinyint"   },   // [0, 255]
        { FieldType.Boolean,       "bit"       },
        { FieldType.ShortDateTime, "date"      },
        { FieldType.ByteArray,     "varbinary(MAX)" },
        { FieldType.DateTime,      "datetime2"      },
        { FieldType.LongDateTime,  "datetimeoffset" }
    };

    public override string Create(TableSpace tablespace)
    {
        throw new NotImplementedException();
    }

    protected override string GetDataType(Field field) => GetDataType(_dataType[field.Type], field.Type, field.Size, VarcharMaxSize);

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

    public override DatabaseProvider Provider => DatabaseProvider.SqlServer;


}
