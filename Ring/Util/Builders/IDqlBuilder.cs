using Ring.Data;
using Ring.Schema.Models;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal interface IDqlBuilder : ISqlBuilder
{
    void Init(DbSchema schema, string[] tableIndex);
    string SelectFrom(Table table);
    string Exists(Table table);
    void AppendFilter(int index, Field field, Operator operatorType, StringBuilder selectFrom);
}
