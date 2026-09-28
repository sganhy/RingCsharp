using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders;

internal interface ISqlBuilder
{
	Encoding ClientEncoding { get; }
	DatabaseProvider Provider { get; }
}
