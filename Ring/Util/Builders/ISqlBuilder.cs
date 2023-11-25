using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Util.Builders;

internal interface ISqlBuilder
{
    DatabaseProvider Provider { get; }
}
