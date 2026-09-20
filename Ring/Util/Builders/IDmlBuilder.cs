using Ring.Schema.Models;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal interface IDmlBuilder : ISqlBuilder
{
	void Init(DbSchema schema);
	string Insert(Table table);
	ReadOnlySpan<byte> Insert(Table table, Encoding encoding);
	string Delete(Table table);
	string Update(Table table);
}
