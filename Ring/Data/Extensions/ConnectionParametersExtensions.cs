using Ring.Data.Models;
using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class ConnectionParametersExtensions
{
	internal static int Hash(this ConnectionParameters connParameter)
	{
		// // Code size: 24 (0x18)
		var hash = new HashCode();
		hash.AddConnParameter(connParameter);
		return hash.ToHashCode();
	}

}
