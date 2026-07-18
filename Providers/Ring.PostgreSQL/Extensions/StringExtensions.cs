using Ring.Data.Models;
using Ring.Schema.Enums;
using Ring.Util.Extensions;

namespace Ring.PostgreSQL.Extensions;

internal static class StringExtensions
{
    //TODO unit test !!!!!!!!!!
	internal static ConnectionParameters ToConnectionParameters(this string connectionString, string clientEncoding = "UTF8")
	{
		// cache ==> ConnectionParameters objects
		var dico = connectionString.GetConnectionParameters(true);
		dico.TryGetValue("HOST", out var host);
		dico.TryGetValue("PORT", out var portStr);
		var port = int.Parse(portStr ?? "5432"); // default port!

		// Accept several common username key variants
		dico.TryGetValue("USER ID", out var userName);
		if (string.IsNullOrEmpty(userName)) dico.TryGetValue("USER", out userName);
		if (string.IsNullOrEmpty(userName)) dico.TryGetValue("USERNAME", out userName);
				
		dico.TryGetValue("PASSWORD", out var password);
		var timeOut = 30000; // 30 seconds
		var applicationName = "Ring"; // 30 seconds
		var clientEncodingParam = "UTF8"; // default client encoding
		dico.TryGetValue("DATABASE", out var databaseName);
		if (!string.IsNullOrEmpty(clientEncoding)) clientEncodingParam = clientEncoding;
		var result = new ConnectionParameters(DatabaseProvider.PostgreSql, host ?? string.Empty, databaseName ?? string.Empty, 
			port, userName ?? string.Empty, password ?? string.Empty, timeOut, applicationName, clientEncodingParam, 0);
		return result;
	}
	
}
