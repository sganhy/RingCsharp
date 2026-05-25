using Ring.Schema.Enums;

namespace Ring.Data.Models;

internal sealed class ConnectionParameters
{
	internal readonly string Host;
	internal readonly int Port;
	internal readonly DatabaseProvider DatabaseProvider;
	internal readonly string UserName;
	internal readonly string Password;
	internal readonly int TimeOut;

	internal ConnectionParameters(DatabaseProvider databaseProvider, string host, int port, string userName, string password, int timeOut)
	{
		Host = host;
		Port = port;
		DatabaseProvider = databaseProvider;
		UserName = userName;
		Password = password;
		TimeOut = timeOut;
	}
}
