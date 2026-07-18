using Ring.Schema.Enums;

namespace Ring.Data.Models;

internal sealed class ConnectionParameters
{
	internal readonly string Host;
	internal readonly int Port;
	internal readonly string DatabaseName;
	internal readonly DatabaseProvider DatabaseProvider;
	internal readonly string UserName; // cannot be null!
	internal readonly string Password;
	internal readonly string ApplicationName;
	internal readonly string ClientEncoding;
	internal readonly int TimeOut;

	internal ConnectionParameters(DatabaseProvider databaseProvider, string host, string databaseName, int port, string userName, string password, int timeOut, string applicationName, string clientEncoding)
	{
		Host = host;
		Port = port;
		DatabaseName = databaseName;
		DatabaseProvider = databaseProvider;
		UserName = userName;
		Password = password;
		ApplicationName = applicationName;
		ClientEncoding = clientEncoding;
		TimeOut = timeOut;
	}
}
