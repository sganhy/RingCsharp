using Ring.PostgreSQL.Exceptions;
using System.Net;
using System.Net.Sockets;

namespace Ring.PostgreSQL.Helpers;

internal static class SocketHelper
{
	internal static Socket ConnectSocket(string host, int port, int timeoutMs)
	{
		IPAddress[] addresses;
		try
		{
			addresses = Dns.GetHostAddresses(host);
		}
		catch (SocketException ex)
		{
			throw new PgOperationalError($"Could not resolve host '{host}': {ex.Message}", "08001", "FATAL", "", "");
		}

		if (addresses.Length == 0)
			throw new PgOperationalError($"Could not resolve host '{host}'.", "08001", "FATAL", "", "");

		var perAddressTimeoutMs = timeoutMs > 0 ? Math.Max(1, timeoutMs / addresses.Length) : -1;

		for (var i = 0; i < addresses.Length; i++)
		{
			var endpoint = new IPEndPoint(addresses[i], port);
			var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { Blocking = false };

			try
			{
				try
				{
					socket.Connect(endpoint);
				}
				catch (SocketException e) when (e.SocketErrorCode == SocketError.WouldBlock)
				{
					// expected: non-blocking connect doesn't complete immediately
				}

				var writable = new List<Socket> { socket };
				var errored = new List<Socket> { socket };
				var selectTimeoutUs = perAddressTimeoutMs < 0 ? -1 : perAddressTimeoutMs * 1000; // Select wants microseconds
				Socket.Select(null, writable, errored, selectTimeoutUs);

				var socketError = (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error)!;
				if (socketError != 0) throw new SocketException(socketError);
				if (writable.Count == 0) throw new TimeoutException();

				socket.Blocking = true;
				return socket;
			}
			catch (Exception e)
			{
				socket.Dispose();
				if (i == addresses.Length - 1)
				{
					var detail = e is TimeoutException
						? $"Connection to {host}:{port} timed out after {timeoutMs} ms."
						: $"Connection to {host}:{port} ({addresses[i]}) failed: {e.Message}";
					throw new PgOperationalError(detail, "08001", "FATAL", "", "");
				}
			}
		}

		throw new PgOperationalError($"Connection to {host}:{port} failed.", "08001", "FATAL", "", "");
	}

}
