using Ring.Schema.Enums;
using Ring.Schema.Extensions;
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
        { FieldType.Date, "TEXT"    },
        { FieldType.ByteArray,     "BLOB"    },
        { FieldType.DateTime,      "TEXT"    },
        { FieldType.DateTimeOffset,  "TEXT"    }
    };

    public DdlBuilder() : base() { }

    public sealed override string Create(TableSpace tablespace) => string.Empty; // no tablespace on SQLite

    protected sealed override Dictionary<FieldType, string> DataType => _dataType;
    public sealed override DatabaseProvider Provider => _currentProvider;
    protected sealed override string MtmPrefix => TableType.Mtm.GetLogicalName(); // physical name prefix for many-to-many tables;
	protected sealed override string? TimeZoneOffsetPrefix => null;
    protected sealed override int VarcharMaxSize => -1;
    protected sealed override string SchemaSeparator => ".";
	protected sealed override char PhysSpecialEntityPrefix => TableType.NonBusinessTable.GetLogicalName()[0];
	protected sealed override string StringCollateInformation => string.Empty;
    protected sealed override string StartPhysicalNameDelimiter => "\"";
    protected sealed override string EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
    protected sealed override string TablePrefix => DefaultTablePrefix;
    protected sealed override string SearchableFieldPrefix => "s_";
	protected sealed override string AlterColumnStatment => "ALTER COLUMN";
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
