using Ring.PostgreSQL.Enums;

namespace Ring.PostgreSQL.Extensions;

internal static class IntExtensions
{
	// authentication type constants
	private const int AuthTypeOk = (int)AuthenticationType.Ok;
	private const int AuthTypeCleartextPassword = (int)AuthenticationType.CleartextPassword;
	private const int AuthTypeMD5Password = (int)AuthenticationType.MD5Password;
	private const int AuthTypeGSS =	(int)AuthenticationType.GSS;
	private const int AuthTypeGSSContinue = (int)AuthenticationType.GSSContinue;
	private const int AuthTypeSSPI = (int)AuthenticationType.SSPI;
	private const int AuthTypeSASL = (int)AuthenticationType.SASL;
	private const int AuthTypeSASLContinue = (int)AuthenticationType.SASLContinue;
	private const int AuthTypeSASLFinal = (int)AuthenticationType.SASLFinal;

	public static AuthenticationType ToAuthenticationType(this int value)
	{
		// Code size: 88 (0x58)
		switch (value)
		{
			case AuthTypeOk: return AuthenticationType.Ok;
			case AuthTypeCleartextPassword:	return AuthenticationType.CleartextPassword;
			case AuthTypeMD5Password:return AuthenticationType.MD5Password;
			case AuthTypeGSS: return AuthenticationType.GSS;
			case AuthTypeGSSContinue: return AuthenticationType.GSSContinue;
			case AuthTypeSSPI: return AuthenticationType.SSPI;
			case AuthTypeSASL: return AuthenticationType.SASL;
			case AuthTypeSASLContinue: return AuthenticationType.SASLContinue;
			case AuthTypeSASLFinal:	return AuthenticationType.SASLFinal;
		}
		return AuthenticationType.Undefined;
	}
}
