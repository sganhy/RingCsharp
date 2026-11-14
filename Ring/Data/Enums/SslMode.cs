namespace Ring.Data.Enums;

/// <summary>Same meaning as libpq's <c>sslmode</c> (the "allow" mode is deliberately not offered).</summary>
internal enum SslMode : byte
{
	/// <summary>Never use TLS. The password and all data travel in clear text.</summary>
	Disable = 0,

	/// <summary>Use TLS if the server offers it, otherwise continue in clear text. Not authenticated: protects against passive sniffing only.</summary>
	Prefer = 1,

	/// <summary>TLS is mandatory (fails if the server refuses it) but the server certificate is NOT validated: protects against passive sniffing only.</summary>
	Require = 2,

	/// <summary>TLS is mandatory and the certificate chain must be trusted; the host name is not checked.</summary>
	VerifyCa = 3,

	/// <summary>TLS is mandatory, the certificate chain must be trusted and must match the host name. The only mode that resists a man-in-the-middle.</summary>
	VerifyFull = 4,
}
