using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders.SQLite;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly static DatabaseProvider _currentProvider = DatabaseProvider.SqlLite;
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

    public DdlBuilder() : base() { }

    public override string Create(TableSpace tablespace) => string.Empty; // no tablespace on SQLite

    protected override Dictionary<FieldType, string> DataType => _dataType;


    protected override string GetPhysicalName(TableSpace tablespace)
    {
        throw new NotImplementedException();
    }
    public override DatabaseProvider Provider => _currentProvider;
    protected override string MtmPrefix => "@mtm_";
    protected override int VarcharMaxSize => -1;
    protected override char SchemaSeparator => '.';
    protected override string StringCollateInformation => string.Empty;
    protected override char StartPhysicalNameDelimiter => '\"';
    protected override char EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
    protected override string TablePrefix => DefaultTablePrefix;

}
