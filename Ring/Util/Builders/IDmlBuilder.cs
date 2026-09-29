using Ring.Schema.Models;
using Ring.Util.Models;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal interface IDmlBuilder : ISqlBuilder
{
	void Init(DbSchema schema);
	SqlEntry Insert(Table table);
	string Delete(Table table);
	string Update(Table table);
}
