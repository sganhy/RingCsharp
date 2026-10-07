using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Exceptions;
using Ring.PostgreSQL.Extensions;
using Ring.PostgreSQL.Models;

namespace Ring.PostgreSQL.Helpers;

internal static class AuthenticationHelper
{

	internal static async ValueTask<(int? BackendPid, int? BackendSecret)> HandleAuthenticationAsync(PipeReader reader, PipeWriter writer, string host, string username, string password, 
		string kerberosServiceName, CancellationToken cancellationToken = default)
	{
		int? backendPid = null;
		int? backendSecret = null;

		using var session = new AuthSession(writer, host, username, password, kerberosServiceName);

		while (true)
		{
			var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
			var buffer = result.Buffer;

			if (buffer.IsEmpty && result.IsCompleted)
			{
				ThrowUnexpectedEof();
			}

			if (!TryProcessAuthMessage(buffer, out var messageLength))
			{
				// a partial message that will never be completed would otherwise spin forever
				if (result.IsCompleted) ThrowUnexpectedEof();

				reader.AdvanceTo(buffer.Start, buffer.End);
				continue;
			}

			var (consumed, isComplete, pid, secret) = await ProcessAuthMessageDetailsAsync(buffer, messageLength, session, cancellationToken).ConfigureAwait(false);

			if (pid.HasValue) backendPid = pid;
			if (secret.HasValue) backendSecret = secret;

			reader.AdvanceTo(consumed);

			if (isComplete)
			{
				return (backendPid, backendSecret);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool TryProcessAuthMessage(ReadOnlySequence<byte> buffer, out int messageLength)
	{
		if (buffer.Length < 5)
		{
			messageLength = 0;
			return false;
		}

		Span<byte> header = stackalloc byte[5];
		buffer.Slice(0, 5).CopyTo(header);

		messageLength = BinaryPrimitives.ReadInt32BigEndian(header.Slice(1, 4));
		return buffer.Length >= messageLength + 1;
	}

	private static async ValueTask<(SequencePosition Consumed, bool IsComplete, int? Pid, int? Secret)> ProcessAuthMessageDetailsAsync(
		ReadOnlySequence<byte> buffer,
		int messageLength,
		AuthSession session,
		CancellationToken cancellationToken)
	{
		Span<byte> header = stackalloc byte[5];
		buffer.Slice(0, 5).CopyTo(header);

		var messageType = (BackendMessageCode)header[0];
		var messagePayload = buffer.Slice(5, messageLength - 4);
		var isComplete = false;
		int? pid = null;
		int? secret = null;

		switch (messageType)
		{
			case BackendMessageCode.AuthenticationRequest:
				await ProcessAuthenticationRequestAsync(messagePayload, session, cancellationToken).ConfigureAwait(false);
				break;

			case BackendMessageCode.BackendKeyData:
				ProcessBackendKeyData(messagePayload, out pid, out secret);
				break;

			case BackendMessageCode.ReadyForQuery:
				isComplete = true;
				break;

			case BackendMessageCode.ErrorResponse:
				var error = messagePayload.IsSingleSegment
					? messagePayload.FirstSpan.ParseErrorFields()
					: messagePayload.ToArray().AsSpan().ParseErrorFields();
				throw new PgOperationalError(error.Message);

			default:
				break;
		}

		var nextPosition = buffer.GetPosition(messageLength + 1);
		return (nextPosition, isComplete, pid, secret);
	}

	private static async ValueTask ProcessAuthenticationRequestAsync(
		ReadOnlySequence<byte> payload,
		AuthSession session,
		CancellationToken cancellationToken)
	{
		Span<byte> authTypeBuffer = stackalloc byte[4];
		payload.Slice(0, 4).CopyTo(authTypeBuffer);
		var authType = (AuthenticationType)BinaryPrimitives.ReadInt32BigEndian(authTypeBuffer);

		switch (authType)
		{
			case AuthenticationType.Ok:
				// A server must not skip the SCRAM final message: otherwise it was never proven to know our verifier.
				if (session.Scram is { IsServerVerified: false })
					ThrowScramIncomplete();
				break;

			case AuthenticationType.CleartextPassword:
				await session.Writer.SendPasswordMessageAsync(session.Password, cancellationToken).ConfigureAwait(false);
				break;

			case AuthenticationType.MD5Password:
				await SendMd5Async(payload, session, cancellationToken).ConfigureAwait(false);
				break;

			case AuthenticationType.SASL:
				await StartScramAsync(payload, session, cancellationToken).ConfigureAwait(false);
				break;

			case AuthenticationType.SASLContinue:
				await ContinueScramAsync(payload, session, cancellationToken).ConfigureAwait(false);
				break;

			case AuthenticationType.SASLFinal:
				FinishScram(payload, session);
				break;

			case AuthenticationType.GSS:
			case AuthenticationType.SSPI:
				await StartNegotiateAsync(authType, session, cancellationToken).ConfigureAwait(false);
				break;

			case AuthenticationType.GSSContinue:
				await ContinueNegotiateAsync(payload, session, cancellationToken).ConfigureAwait(false);
				break;

			default:
				ThrowUnsupportedAuthType(authType);
				break;
		}
	}

	#region SASL (SCRAM-SHA-256)

	private static async ValueTask StartScramAsync(ReadOnlySequence<byte> payload, AuthSession session, CancellationToken cancellationToken)
	{
		// Payload after the int32 auth type: list of NUL-terminated mechanism names, ended by an empty string.
		var mechanisms = payload.Slice(4).ToArray();

		var found = false;
		var start = 0;
		for (var i = 0; i < mechanisms.Length; i++)
		{
			if (mechanisms[i] != 0) continue;
			if (i == start) break; // terminating empty string

			var name = Encoding.UTF8.GetString(mechanisms, start, i - start);
			if (string.Equals(name, ScramSha256Client.MechanismName, StringComparison.Ordinal))
				found = true;
			start = i + 1;
		}

		if (!found) ThrowNoSupportedSaslMechanism();

		session.Scram?.Dispose();
		session.Scram = new ScramSha256Client(session.Password);

		var clientFirst = session.Scram.CreateClientFirstMessage();
		await session.Writer.SendSASLInitialResponseAsync(ScramSha256Client.MechanismName, clientFirst, cancellationToken).ConfigureAwait(false);
	}

	private static async ValueTask ContinueScramAsync(ReadOnlySequence<byte> payload, AuthSession session, CancellationToken cancellationToken)
	{
		if (session.Scram is null) ThrowUnexpectedAuthMessage(AuthenticationType.SASLContinue);

		var serverFirst = payload.Slice(4).ToArray();
		var clientFinal = session.Scram.ProcessServerFirst(serverFirst);

		// carries the client proof: zeroed in the pipe buffer after the flush
		await session.Writer.SendSASLResponseAsync(clientFinal, cancellationToken).ConfigureAwait(false);
		CryptographicOperations.ZeroMemory(clientFinal);
	}

	private static void FinishScram(ReadOnlySequence<byte> payload, AuthSession session)
	{
		if (session.Scram is null) ThrowUnexpectedAuthMessage(AuthenticationType.SASLFinal);

		session.Scram.VerifyServerFinal(payload.Slice(4).ToArray());
	}

	#endregion

	#region GSS / SSPI (Kerberos / Negotiate)

	// GSS (7): Kerberos through GSSAPI (libgssapi_krb5 on Linux/macOS, SSPI on Windows).
	// SSPI (9): server asks for Windows-style Negotiate (Kerberos with NTLM fallback).
	// Both use the same wire flow: client token in a 'p' message, server tokens in AuthenticationGSSContinue,
	// then AuthenticationOk.
	private static async ValueTask StartNegotiateAsync(AuthenticationType authType, AuthSession session, CancellationToken cancellationToken)
	{
		session.Negotiate?.Dispose();
		session.Negotiate = new NegotiateAuthentication(new NegotiateAuthenticationClientOptions
		{
			Package = authType == AuthenticationType.GSS ? "Kerberos" : "Negotiate",
			TargetName = $"{session.KerberosServiceName}/{session.Host}", // SPN: use a host name, not an IP address
			RequiredProtectionLevel = ProtectionLevel.None                 // PostgreSQL only uses GSS for authentication
		});

		var token = session.Negotiate.GetOutgoingBlob(ReadOnlySpan<byte>.Empty, out var status);
		ThrowIfNegotiateFailed(status);

		if (token is { Length: > 0 })
			await session.Writer.SendSASLResponseAsync(token, cancellationToken).ConfigureAwait(false);
	}

	private static async ValueTask ContinueNegotiateAsync(ReadOnlySequence<byte> payload, AuthSession session, CancellationToken cancellationToken)
	{
		if (session.Negotiate is null) ThrowUnexpectedAuthMessage(AuthenticationType.GSSContinue);

		var serverToken = payload.Slice(4).ToArray();
		var token = session.Negotiate.GetOutgoingBlob(serverToken, out var status);
		ThrowIfNegotiateFailed(status);

		// After the last server token there may be nothing left to send; the server then answers AuthenticationOk.
		if (token is { Length: > 0 })
			await session.Writer.SendSASLResponseAsync(token, cancellationToken).ConfigureAwait(false);
	}

	private static void ThrowIfNegotiateFailed(NegotiateAuthenticationStatusCode status)
	{
		if (status is NegotiateAuthenticationStatusCode.Completed or NegotiateAuthenticationStatusCode.ContinueNeeded)
			return;

		throw new InvalidOperationException($"GSS/SSPI authentication failed: {status}.");
	}

	#endregion

	// Non-async on purpose: the stackalloc'd salt span must not live in a state machine.
	private static ValueTask SendMd5Async(ReadOnlySequence<byte> payload, AuthSession session, CancellationToken cancellationToken)
	{
		Span<byte> salt = stackalloc byte[4];
		payload.Slice(4, 4).CopyTo(salt);
		return session.Writer.SendMd5PasswordAsync(session.Username, session.Password, salt, cancellationToken);
	}

	private static void ProcessBackendKeyData(ReadOnlySequence<byte> payload, out int? pid, out int? secret)
	{
		Span<byte> data = stackalloc byte[8];
		payload.Slice(0, 8).CopyTo(data);

		pid = BinaryPrimitives.ReadInt32BigEndian(data[..4]);
		secret = BinaryPrimitives.ReadInt32BigEndian(data.Slice(4, 4));
	}

	[DoesNotReturn]
	private static void ThrowUnexpectedEof() =>
		throw new InvalidOperationException("Connection closed by server during authentication handshake.");

	[DoesNotReturn]
	private static void ThrowNoSupportedSaslMechanism() =>
		throw new NotSupportedException($"The server offers no supported SASL mechanism (supported: {ScramSha256Client.MechanismName}).");

	[DoesNotReturn]
	private static void ThrowScramIncomplete() =>
		throw new InvalidOperationException("Server sent AuthenticationOk without completing the SCRAM exchange.");

	[DoesNotReturn]
	private static void ThrowUnexpectedAuthMessage(AuthenticationType authType) =>
		throw new InvalidOperationException($"Unexpected authentication message '{authType}' in this state.");

	[DoesNotReturn]
	private static void ThrowUnsupportedAuthType(AuthenticationType authType) =>
		throw new NotSupportedException($"PostgreSQL authentication type '{authType}' is not supported.");
}
