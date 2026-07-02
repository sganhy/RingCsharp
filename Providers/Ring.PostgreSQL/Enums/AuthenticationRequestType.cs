namespace Ring.PostgreSQL.Enums;

internal enum AuthenticationRequestType
{
	Ok = 0,
	CleartextPassword = 3,
	MD5Password = 5,
	GSS = 7,
	GSSContinue = 8,
	SSPI = 9,
	SASL = 10,
	SASLContinue = 11,
	SASLFinal = 12
}
