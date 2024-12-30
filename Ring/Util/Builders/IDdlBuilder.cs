using Ring.Schema.Models;
using Index = Ring.Schema.Models.Index;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Schema.Enums;

namespace Ring.Util.Builders;

internal interface IDdlBuilder : ISqlBuilder
{
    string Create(DbSchema schema);
    string Create(TableSpace tablespace);
    string Create(Table table, TableSpace? tablespace = null);
    string Create(Index index, Table table, TableSpace? tablespace = null);
    string Create(Constraint constraint, TableSpace? tablespace = null);
    string Drop(Table table);
    string AlterAddColumn(Table table, IColumn column);
    string AlterDropColumn(Table table, IColumn column);
    string Truncate(Table table);
    string GetSecondColumn(Field field); // some fields are defined onto 2 columns; return the physical name

    string GetPhysicalName(Constraint constraint);
    string GetPhysicalName(Table table, DbSchema schema);
    string GetPhysicalName(Index index, Table table);
    string GetPhysicalName(EntityType entityType, string name);
}
