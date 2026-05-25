namespace Ring.Data.Enums;

internal enum AuthenticationType : byte
{
	// Plaintext password.
	Password = 1,
	// MD5 hashed password.
	MD5 = 2,
	// Kerberos.
	GSS = 3,
	// Windows SSPI.
	SSPI = 4,
	// SASL.
	ScramSHA256 = 5,
	// No authentication exchange.
	None = 6,
}
