using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Text;

namespace Ring.Util.Builders.MariaDB;

internal sealed class DqlBuilder : BaseDqlBuilder
{
	internal DqlBuilder(Encoding clientEncoding) : base(DatabaseProvider.MariaDb, clientEncoding) {}
    protected sealed override string GetSelection(in Column column) => column.PhysicalName;

}
