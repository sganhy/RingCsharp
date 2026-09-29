using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Text;

namespace Ring.Util.Builders.Oracle;

internal sealed class DqlBuilder : BaseDqlBuilder
{
    internal DqlBuilder(Encoding clientEncoding, bool logSql) : base(DatabaseProvider.Oracle, clientEncoding, logSql) {}
    protected sealed override string GetSelection(in Column column) => column.PhysicalName;

}
