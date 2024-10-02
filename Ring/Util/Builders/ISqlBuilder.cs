using Ring.Schema.Enums;

namespace Ring.Util.Builders;

internal interface ISqlBuilder
{
    DatabaseProvider Provider { get; }
}
