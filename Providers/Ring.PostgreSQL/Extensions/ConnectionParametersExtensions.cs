using Ring.Data.Enums;
using Ring.Data.Models;

namespace Ring.PostgreSQL.Extensions;

internal static class ConnectionParametersExtensions
{
	internal static string GetParameterName(this ConnectionParameters? _, ConnectionParametersType parameterType)
	{
		// Code size: 124 (0x7c)
		return parameterType switch
		{
			ConnectionParametersType.DataBase => "database",
			ConnectionParametersType.Host => "host",
			ConnectionParametersType.UserName => "user",
			ConnectionParametersType.Password => "password",
			ConnectionParametersType.ClientEncoding => "client_encoding",
			ConnectionParametersType.TimeOut => "timeout",
			ConnectionParametersType.ApplicationName => "application_name",
			ConnectionParametersType.Port => "port",
			_ => throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null)
		};
	}

	internal static ConnectionParameters Set(this ConnectionParameters parameter, int id, int sqlSendBufferSize)
	{
		// DatabaseProvider databaseProvider, string host, string databaseName, int port, string userName, string password, int timeOut, string applicationName, string clientEncoding, int sqlSendBufferSize
		var newParameter = new ConnectionParameters(
			parameter.DatabaseProvider,
			parameter.Host,
			parameter.DatabaseName,
			parameter.Port,
			parameter.UserName,
			parameter.Password, 
			parameter.TimeOut,
			$"{parameter.ApplicationName} ({id})",
			parameter.ClientEncoding,
			sqlSendBufferSize
		);
		return newParameter;
	}
}
