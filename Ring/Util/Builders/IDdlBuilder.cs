using Ring.Schema.Models;
using Index = Ring.Schema.Models.Index;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Schema.Enums;

namespace Ring.Util.Builders;

internal interface IDdlBuilder : ISqlBuilder
{
	string AlterAddColumn(Table table, in Column column);
	string AlterDropColumn(Table table, in Column column);
	string Create(DbSchema schema);
	string Create(TableSpace tablespace);
	string Create(Table table, TableSpace? tablespace = null);
	string Create(Index index, Table table, TableSpace? tablespace = null);
	string Create(Constraint constraint, TableSpace? tablespace = null);
	string Drop(Table table);
	string GetPhysicalName(Table table, DbSchema schema);
	string GetPhysicalName(Index index, Table table);
	string GetPhysicalName(Field field, Table table);
	string GetPhysicalName(EntityType entityType, string name);
	Constraint[] GetConstraints(Table table);
	bool HasTimeZoneOffsetColumn { get; }
	string Truncate(Table table);
}
