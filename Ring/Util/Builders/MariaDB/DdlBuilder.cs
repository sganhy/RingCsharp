using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using MySqlDdlBuilder = Ring.Util.Builders.MySQL.DdlBuilder;

namespace Ring.Util.Builders.MariaDB;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly DatabaseProvider _currentProvider = DatabaseProvider.MariaDb;
	private readonly MySqlDdlBuilder _mySqlDdlBuilder = new();

    public DdlBuilder() : base() {}
    public sealed override string Create(TableSpace tablespace) => tablespace.Name;
    public sealed override DatabaseProvider Provider => _currentProvider;
    protected sealed override string MtmPrefix => TableType.Mtm.GetLogicalName(); // physical name prefix for many-to-many tables
	protected sealed override string? TimeZoneOffsetPrefix => null;
    protected sealed override Dictionary<FieldType, string> DataType => _mySqlDdlBuilder.ProviderDataType;
    protected sealed override int VarcharMaxSize => 65535;
    protected sealed override string StringCollateInformation => throw new NotImplementedException();
    protected sealed override string SchemaSeparator => ".";
	protected sealed override char PhysSpecialEntityPrefix => TableType.NonBusinessTable.GetLogicalName()[0];
	protected sealed override string StartPhysicalNameDelimiter => "`";
    protected sealed override string EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
    protected sealed override string TablePrefix => DefaultTablePrefix;
	protected sealed override string AlterColumnStatment => string.Empty;
	protected sealed override string SearchableFieldPrefix => "s_";
    protected override string GetCatalogPhysicalName(TableType tableType) => string.Empty;
	protected override string GetSchemaPhysicalName(TableType tableType) 
    {
        return string.Empty;
    }
	protected override string GetPhysicalName(TableType tableType, Field field)
	{
        return string.Empty;
	}

    protected override Constraint? HasCheckConstraint(Table table, Column column) => null;
}
