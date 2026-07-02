using Ring.PostgreSQL.Enums;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class NetworkStreamExtensions
{
	// Diagnostic helper: prints a hex dump of a buffer to the console.
	private static void LogHex(string title, byte[] buffer)
	{
		var sb = new StringBuilder(buffer.Length * 3);
		for (var i = 0; i < buffer.Length; i++)
		{
			sb.Append(buffer[i].ToString("x2"));
			if (i + 1 < buffer.Length) sb.Append(' ');
		}
		Console.WriteLine($"{title}: {buffer.Length} bytes -> {sb}");
	}

	internal static async Task<(byte Code, byte[] Body)> ReadMessageAsync(this NetworkStream stream, CancellationToken cancellationToken = default)
	{
		var header = new byte[5];
		await stream.ReadExactlyAsync(header.AsMemory(), cancellationToken).ConfigureAwait(false);

		var code = header[0];
		var bodyLength = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(1)) - 4;
		if (bodyLength < 0)
			throw new InvalidOperationException("Invalid message length received from server.");

		var body = bodyLength > 0 ? new byte[bodyLength] : Array.Empty<byte>();
		if (bodyLength > 0)
			await stream.ReadExactlyAsync(body.AsMemory(), cancellationToken).ConfigureAwait(false);

		// combined buffer for logging
		var combined = new byte[5 + body.Length];
		Array.Copy(header, 0, combined, 0, 5);
		if (body.Length > 0) Array.Copy(body, 0, combined, 5, body.Length);
		LogHex($"ServerMessage (code={(char)code})", combined);

		return (code, body);
	}

	internal static async Task<(int? BackendPid, int? BackendSecret)> WaitUntilReadyAsync(this NetworkStream stream, CancellationToken cancellationToken = default)
	{
		int? pid = null;
		int? secret = null;
		while (true)
		{
			var (code, body) = await stream.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
			switch ((BackendMessageCode)code)
			{
				case BackendMessageCode.BackendKeyData:
					if (body.Length >= 8)
					{
						pid = BinaryPrimitives.ReadInt32BigEndian(body.AsSpan(0, 4));
						secret = BinaryPrimitives.ReadInt32BigEndian(body.AsSpan(4, 4));
					}
					continue;
				case BackendMessageCode.ParameterStatus:
				case BackendMessageCode.NoticeResponse:
					continue;
				case BackendMessageCode.ReadyForQuery:
					return (pid, secret);
				case BackendMessageCode.ErrorResponse:
					//throw ParseErrorResponse(body);
					break;
				default:
					break;
			}
		}
	}

	internal static async Task ReadExactlyAsync(this NetworkStream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
	{
		var pos = 0;
		var len = buffer.Length;
		while (pos < len)
		{
			var read = await stream.ReadAsync(buffer.Slice(pos), cancellationToken).ConfigureAwait(false);
			if (read == 0) throw new EndOfStreamException("Unexpected end of stream while reading message.");
			pos += read;
		}
	}

	internal static (byte Code, byte[] Body) ReadMessage(this NetworkStream stream)
	{
		Span<byte> header = stackalloc byte[5];
		stream.ReadExactly(header);

		var code = header[0];
		var bodyLength = BinaryPrimitives.ReadInt32BigEndian(header[1..]) - 4;
		if (bodyLength < 0)
			throw new InvalidOperationException("Invalid message length received from server.");

		var body = bodyLength > 0 ? new byte[bodyLength] : Array.Empty<byte>();
		if (bodyLength > 0) stream.ReadExactly(body);

		// Build a combined buffer for logging (header + body)
		var headerCopy = new byte[5];
		header.CopyTo(headerCopy);
		var combined = new byte[5 + body.Length];
		Array.Copy(headerCopy, 0, combined, 0, 5);
		if (body.Length > 0) Array.Copy(body, 0, combined, 5, body.Length);
		LogHex($"ServerMessage (code={(char)code})", combined);

		return (code, body);
	}

	internal static void SendSASLResponse(this NetworkStream stream, byte[] data)
	{
		// Write message type 'p', message length, then the SASL response bytes.
		// The length field is the length of the message contents including
		// the length field itself (4) but excluding the type byte.
		var length = 4 + data.Length; // 4 bytes for the length field + data
		var buffer = new byte[1 + length];
		buffer[0] = (byte)'p';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);

		// Copy the SASL response directly after the length field
		data.CopyTo(buffer.AsSpan(5));

		LogHex("SASLResponse (client)", buffer);
		LogHex("SASLResponse (client)", buffer);
		stream.Write(buffer);
		stream.Flush();
	}

	internal static async Task SendSASLResponseAsync(this NetworkStream stream, byte[] data, CancellationToken cancellationToken = default)
	{
		var length = 4 + data.Length; // 4 bytes for the length field + data
		var buffer = new byte[1 + length];
		buffer[0] = (byte)'p';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);
		data.CopyTo(buffer.AsSpan(5));
		LogHex("SASLResponse (client)", buffer);
		await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     After AuthenticationOk, the server sends BackendKeyData and
	///     ParameterStatus messages before ReadyForQuery signals the
	///     connection is ready for use.
	/// </summary>
	internal static (int? BackendPid, int? BackendSecret) WaitUntilReady(this NetworkStream stream)
	{
		int? pid = null;
		int? secret = null;
		while (true)
		{
			var (code, body) = stream.ReadMessage();
			switch ((BackendMessageCode)code)
			{
				case BackendMessageCode.BackendKeyData:
					// BackendKeyData: 4-byte process id, 4-byte secret key
					if (body.Length >= 8)
					{
						pid = BinaryPrimitives.ReadInt32BigEndian(body.AsSpan(0, 4));
						secret = BinaryPrimitives.ReadInt32BigEndian(body.AsSpan(4, 4));
					}
					continue;
				case BackendMessageCode.ParameterStatus:
				case BackendMessageCode.NoticeResponse:
					continue; // capture process id / secret key / server params here if you need them later
				case BackendMessageCode.ReadyForQuery:
					return (pid, secret);
				case BackendMessageCode.ErrorResponse:
					//throw ParseErrorResponse(body);
					break;
				default:
					//throw UnexpectedMessage(code, "BackendKeyData, ParameterStatus, or ReadyForQuery");
					break;
			}
		}
	}

	internal static async Task SendPasswordMessageAsync(this NetworkStream stream, string password, CancellationToken cancellationToken = default)
	{
		var length = 4 + Encoding.UTF8.GetByteCount(password) + 1;
		var buffer = new byte[1 + length];
		buffer[0] = (byte)'p';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), length);
		var written = Encoding.UTF8.GetBytes(password, buffer.AsSpan(5));
		buffer[5 + written] = 0;
		LogHex("PasswordMessage (client)", buffer);
		await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	internal static async Task SendSASLInitialResponseAsync(this NetworkStream stream, string mechanism, byte[] data, CancellationToken cancellationToken = default)
	{
		// Code size: 79 (0x4f)
		var mechanismLength = Encoding.UTF8.GetByteCount(mechanism);
		var outerLength = 4 + mechanismLength + 1 + 4 + data.Length;
		var buffer = new byte[1 + outerLength];
		buffer[0] = (byte)'p';
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(1), outerLength);

		var offset = 5;
		offset += Encoding.UTF8.GetBytes(mechanism, buffer.AsSpan(offset));
		buffer[offset++] = 0;
		BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offset), data.Length);
		offset += 4;
		data.CopyTo(buffer.AsSpan(offset));
		await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

}
