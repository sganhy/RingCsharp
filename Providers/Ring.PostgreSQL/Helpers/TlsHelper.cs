using Ring.Data.Enums;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Ring.PostgreSQL.Helpers;

/// <summary>
/// PostgreSQL TLS negotiation: SSLRequest, one-byte answer, then a TLS handshake on the same socket.
/// Runs on the raw <see cref="NetworkStream"/> BEFORE any pipe exists, so no byte of the answer can sit in a pipe buffer
/// and be mistaken for (or mixed with) TLS data.
/// </summary>
internal static class TlsHelper
{
	// Int32 length (8) + Int32 request code 80877103 (1234 << 16 | 5679).
	private static readonly byte[] SslRequest = [0, 0, 0, 8, 0x04, 0xD2, 0x16, 0x2F];

	/// <summary>Returns the stream the pipes must be built on: <paramref name="stream"/> itself, or an <see cref="SslStream"/> over it.</summary>
	internal static Stream Negotiate(NetworkStream stream, Socket socket, string host, SslMode mode)
	{
		if (mode == SslMode.Disable) return stream;

		stream.Write(SslRequest);
		stream.Flush();

		Span<byte> answer = stackalloc byte[1];
		if (stream.Read(answer) != 1) throw ConnectionClosedDuringNegotiation();
		if (!AcceptsTls(answer[0], socket, mode)) return stream;

		var ssl = new SslStream(stream, leaveInnerStreamOpen: true);
		try
		{
			ssl.AuthenticateAsClient(CreateOptions(host, mode));
			return ssl;
		}
		catch
		{
			ssl.Dispose();
			throw;
		}
	}

	internal static async ValueTask<Stream> NegotiateAsync(NetworkStream stream, Socket socket, string host, SslMode mode, CancellationToken cancellationToken)
	{
		if (mode == SslMode.Disable) return stream;

		await stream.WriteAsync(SslRequest, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

		var answer = new byte[1];
		if (await stream.ReadAsync(answer, cancellationToken).ConfigureAwait(false) != 1) throw ConnectionClosedDuringNegotiation();
		if (!AcceptsTls(answer[0], socket, mode)) return stream;

		var ssl = new SslStream(stream, leaveInnerStreamOpen: true);
		try
		{
			await ssl.AuthenticateAsClientAsync(CreateOptions(host, mode), cancellationToken).ConfigureAwait(false);
			return ssl;
		}
		catch
		{
			await ssl.DisposeAsync().ConfigureAwait(false);
			throw;
		}
	}

	// 'S' = go ahead with TLS, 'N' = the server has no TLS. Anything else is a protocol violation (an old server answering 'E' included).
	private static bool AcceptsTls(byte answer, Socket socket, SslMode mode)
	{
		// CVE-2021-23222: a man-in-the-middle can append plaintext after the one-byte answer. A legitimate server sends nothing more
		// until the client speaks (or handshakes), so any pending byte means the stream can't be trusted.
		if (socket.Available > 0) throw new IOException("Unexpected data received after the SSLRequest answer.");

		switch (answer)
		{
			case (byte)'S':
				return true;
			case (byte)'N' when mode == SslMode.Prefer:
				return false; // allowed to continue in clear text on this same connection
			case (byte)'N':
				throw new AuthenticationException($"SslMode={mode} requires TLS but the server does not support it.");
			default:
				throw new IOException($"Unexpected answer to the SSLRequest: 0x{answer:X2}.");
		}
	}

	private static SslClientAuthenticationOptions CreateOptions(string host, SslMode mode) => new()
	{
		TargetHost = host,
		EnabledSslProtocols = SslProtocols.None, // let the OS choose (TLS 1.2+ on supported platforms)
		CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
		RemoteCertificateValidationCallback = mode switch
		{
			// Prefer / Require: encryption without server authentication (libpq semantics). Say so in the docs: it does not stop a MITM.
			SslMode.Prefer or SslMode.Require => static (_, _, _, _) => true,
			// VerifyCa: the chain must validate; only a host name mismatch is tolerated.
			SslMode.VerifyCa => static (_, _, _, errors) => (errors & ~SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.None,
			// VerifyFull: default validation (chain + host name).
			_ => null,
		},
	};

	private static IOException ConnectionClosedDuringNegotiation() => new("The server closed the connection during the TLS negotiation.");
}
