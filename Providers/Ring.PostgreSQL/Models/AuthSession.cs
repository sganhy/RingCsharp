using Ring.PostgreSQL.Helpers;
using System.IO.Pipelines;
using System.Net.Security;

namespace Ring.PostgreSQL.Models;

/// <summary>State that must survive across the several messages of one SASL / GSS exchange.</summary>
internal sealed class AuthSession : IDisposable
{
	internal readonly PipeWriter Writer;
	internal readonly string Host;
	internal readonly string Username;
	internal readonly string Password;
	internal readonly string KerberosServiceName;

	internal AuthSession(PipeWriter writer, string host, string username, string password, string kerberosServiceName)
	{
		Writer = writer;
		Host = host;
		Username = username;
		Password = password;
		KerberosServiceName = kerberosServiceName;
	}

	internal ScramSha256Client? Scram { get; set; }
	internal NegotiateAuthentication? Negotiate { get; set; }

	public void Dispose()
	{
		Scram?.Dispose();
		Negotiate?.Dispose();
	}
}
