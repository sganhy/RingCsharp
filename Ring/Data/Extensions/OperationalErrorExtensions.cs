using Ring.Data.Models;
using Ring.Util.Extensions;

namespace Ring.Data.Extensions;

internal static class OperationalErrorExtensions
{
	internal static OperationalError Set(this in OperationalError operationalError, in AlterQuery query)
	{
		return operationalError;
	}

	internal static int Hash(this in OperationalError operationalError)
	{
		// Code size: 24 (0x18) - no virtual calls
		var hash = new HashCode();
		hash.AddOperationalError(operationalError);
		return hash.ToHashCode();
	}

	internal static bool IsEquivalentTo(this in OperationalError operationalError, in OperationalError other)
	{
		// Code size: 151 (0x97)
		return operationalError.Type == other.Type &&
			   operationalError.Message == other.Message &&
			   operationalError.SqlState == other.SqlState &&
			   operationalError.Severity == other.Severity &&
			   operationalError.Detail == other.Detail &&
			   operationalError.Hint == other.Hint &&
			   operationalError.TableName == other.TableName &&
			   operationalError.SchemaName == other.SchemaName;
	}

	internal static OperationalError InvalidProtocol(this in OperationalError? _, string message)
		=> new(OperationalErrorType.InvalidProtocol, message, string.Empty, string.Empty, null, null, null, null);
	internal static OperationalError ConnectionClosed(this in OperationalError? _, string message)
		=> new(OperationalErrorType.ConnectionClosed, message, string.Empty, string.Empty, null, null, null, null);

}