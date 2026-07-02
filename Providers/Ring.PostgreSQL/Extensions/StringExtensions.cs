using Ring.Data.Models;
using Ring.Schema.Enums;
using Ring.Util.Extensions;

namespace Ring.PostgreSQL.Extensions;

internal static class StringExtensions
{
    //TODO unit test !!!!!!!!!!
	internal static ConnectionParameters ToConnectionParameters(this string connectionString)
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
		dico.TryGetValue("DATABASE", out var databaseName);
        var result = new ConnectionParameters(DatabaseProvider.PostgreSql, host ?? string.Empty, databaseName ?? string.Empty, 
			port, userName ?? string.Empty, password ?? string.Empty, timeOut);
		return result;
	}

	#region private methods

    #endregion 

}
