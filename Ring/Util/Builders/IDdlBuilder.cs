using Ring.Schema.Models;
using Index = Ring.Schema.Models.Index;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Schema.Enums;

namespace Ring.Util.Builders;

internal interface IDdlBuilder
{
    DatabaseProvider Provider { get; }
    string Create(DbSchema schema);
    string Create(TableSpace tablespace);
    string Create(Table table, TableSpace? tablespace = null);
    string Create(Index index, Table table, TableSpace? tablespace = null);
    string Create(Constraint constraint, TableSpace? tablespace = null);
    string Drop(Table table);
    string AlterAddColumn(Table table, Field field);
    string AlterAddColumn(Table table, Relation relation);
    string AlterDropColumn(Table table, Field field);
    string AlterDropColumn(Table table, Relation relation);
    string Truncate(Table table);
    string GetPhysicalName(Table table, DbSchema schema);
    string GetPhysicalName(Field field);
    string GetPhysicalName(Relation relation);
}
