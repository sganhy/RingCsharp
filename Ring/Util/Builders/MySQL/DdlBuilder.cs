using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using PostgreSqlDdlBuilder = Ring.Util.Builders.PostgreSQL.DdlBuilder;

namespace Ring.Util.Builders.MySQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly DatabaseProvider _currentProvider = DatabaseProvider.MySql;
	private readonly PostgreSqlDdlBuilder _pgDdlBuilder = new();
	private readonly Dictionary<FieldType, string> _dataType = new()
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

    public DdlBuilder() : base() {}
    public sealed override string Create(TableSpace tablespace) => tablespace.Name;
    public sealed override DatabaseProvider Provider => _currentProvider;
    protected sealed override string MtmPrefix => TableType.Mtm.GetLogicalName(); // physical name prefix for many-to-many tables
	protected sealed override string? TimeZoneOffsetPrefix => null;
    protected sealed override Dictionary<FieldType, string> DataType => _dataType;
    protected sealed override int VarcharMaxSize => 65535;
    protected sealed override string StringCollateInformation => throw new NotImplementedException();
    protected sealed override string SchemaSeparator => ".";
	protected sealed override char PhysSpecialEntityPrefix => TableType.NonBusinessTable.GetLogicalName()[0];
	protected sealed override string StartPhysicalNameDelimiter => "`";
    protected sealed override string EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
    protected sealed override string TablePrefix => DefaultTablePrefix;
	protected sealed override string AlterColumnStatment => string.Empty;
	protected sealed override string SearchableFieldPrefix => "s_";
	protected override string GetCatalogPhysicalName(TableType tableType) => _pgDdlBuilder.GetPhysicalName(EntityType.Table, tableType.GetLogicalName()); // use PostgreSQL DDL builder to get the physical name for catalogs
	protected override string GetSchemaPhysicalName(TableType tableType) 
    {
		// Code size: 115 (0x73)
		if (tableType.IsCatalog()) 
        {
			var meta = new Meta(-1, (byte)EntityType.Table, 0, (int)tableType, 0L, tableType.GetLogicalName(), null, null, true);
			var defaultSchema = Meta.GetDefaultSchema(meta, Provider);
			var currentTable = Meta.GetDefaultTable(meta);
			var result = _pgDdlBuilder.GetPhysicalName(currentTable, defaultSchema);  // use PostgreSQL DDL builder to get the physical name for catalogs
            // last operation remove table information
			return result?.IndexOf(SchemaSeparator, StringComparison.Ordinal)>= 0 ? result[..result.IndexOf(SchemaSeparator, StringComparison.Ordinal)] : result;
		}
        return string.Empty;
    }
	protected override string GetPhysicalName(TableType tableType, Field field)
	{
		// Code size: 42 (0x2a)
		var meta = new Meta(-1, (byte)EntityType.Table, 0, (int)tableType, 0L, string.Empty, null, null, true);
		var currentTable = Meta.GetDefaultTable(meta);
		return _pgDdlBuilder.GetPhysicalName(field, currentTable);  // use PostgreSQL DDL builder to get the physical name for catalogs
	}

    protected override Constraint? HasCheckConstraint(Table table, Column column) => null;
}
