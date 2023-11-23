using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.SQLite;

internal sealed class DdlBuilder : BaseDdlBuilder
{

    private const int VarcharMaxSize = -1; // no text size delimiter

    private readonly static Dictionary<FieldType, string> _dataType = new()
    {
        { FieldType.String,        "TEXT"    },
        { FieldType.LongString,    "TEXT"    },
        { FieldType.Double,        "REAL"    },
        { FieldType.Float,         "REAL"    },
        { FieldType.Long,          "INTEGER" },
        { FieldType.Int,           "INTEGER" },
        { FieldType.Short,         "INTEGER" },
        { FieldType.Byte,          "INTEGER" },
        { FieldType.Boolean,       "INTEGER" },
        { FieldType.ShortDateTime, "TEXT"    },
        { FieldType.ByteArray,     "BLOB"    },
        { FieldType.DateTime,      "TEXT"    },
        { FieldType.LongDateTime,  "TEXT"    }
    };

    public override string Create(TableSpace tablespace) => string.Empty; // no tablespace on SQLite
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

    public override DatabaseProvider Provider => DatabaseProvider.SqlLite;

}
