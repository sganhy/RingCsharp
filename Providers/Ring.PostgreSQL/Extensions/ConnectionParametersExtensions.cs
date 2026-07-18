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
}
