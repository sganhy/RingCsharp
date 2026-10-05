using Ring.PostgreSQL.Exceptions;
using System.Net;
using System.Net.Sockets;

namespace Ring.PostgreSQL.Helpers;

internal static class SocketHelper
{
	internal static async ValueTask<Socket> ConnectSocketAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken = default)
	{
		IPAddress[] addresses;
		try
		{
			addresses = await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);
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
			var socket = new Socket(addresses[i].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				using var cts = perAddressTimeoutMs > 0
					? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
					: null;

				cts?.CancelAfter(perAddressTimeoutMs);

				var effectiveToken = cts?.Token ?? cancellationToken;
				await socket.ConnectAsync(addresses[i], port, effectiveToken).ConfigureAwait(false);

				return socket;
			}
			catch (Exception e)
			{
				socket.Dispose();
				if (i == addresses.Length - 1)
				{
					var detail = e is OperationCanceledException
						? $"Connection to {host}:{port} timed out after {timeoutMs} ms."
						: $"Connection to {host}:{port} ({addresses[i]}) failed: {e.Message}";
					throw new PgOperationalError(detail, "08001", "FATAL", "", "");
				}
			}
		}

		throw new PgOperationalError($"Connection to {host}:{port} failed.", "08001", "FATAL", "", "");
	}

}