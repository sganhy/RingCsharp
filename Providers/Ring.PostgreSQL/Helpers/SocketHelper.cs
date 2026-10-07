using Ring.PostgreSQL.Exceptions;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Ring.PostgreSQL.Helpers;

internal static class SocketHelper
{
	/// <summary>
	/// Resolves <paramref name="host"/> and connects to the first address that answers.
	/// <paramref name="timeoutMs"/> is a budget for DNS + all connection attempts together (0 or less = no timeout).
	/// Caller cancellation propagates as OperationCanceledException; everything else is reported as a PgOperationalError.
	/// </summary>
	internal static async ValueTask<Socket> ConnectSocketAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken = default)
	{
		var hasTimeout = timeoutMs > 0;
		var started = Stopwatch.GetTimestamp();

		// 1. DNS (inside the same time budget)
		IPAddress[] addresses;
		using (var dnsCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
		{
			if (hasTimeout) dnsCts.CancelAfter(timeoutMs);

			try
			{
				addresses = await Dns.GetHostAddressesAsync(host, dnsCts.Token).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				throw Fail($"Resolving host '{host}' timed out after {timeoutMs} ms.");
			}
			catch (SocketException ex)
			{
				throw Fail($"Could not resolve host '{host}': {ex.Message}");
			}
		}

		if (addresses.Length == 0)
			throw Fail($"Could not resolve host '{host}'.");

		// 2. Try each address; every attempt gets an equal share of what is left of the budget.
		List<string>? failures = null;

		for (var i = 0; i < addresses.Length; i++)
		{
			var address = addresses[i];
			var attemptMs = Timeout.Infinite;

			if (hasTimeout)
			{
				var remaining = timeoutMs - (int)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
				if (remaining <= 0)
				{
					(failures ??= new()).Add("time budget exhausted");
					break;
				}
				attemptMs = Math.Max(1, remaining / (addresses.Length - i));
			}

			var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

				using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				if (hasTimeout) cts.CancelAfter(attemptMs);

				await socket.ConnectAsync(address, port, cts.Token).ConfigureAwait(false);
				return socket;
			}
			catch (Exception e)
			{
				socket.Dispose();

				// the caller asked to stop: don't try the next address, don't wrap
				if (e is OperationCanceledException && cancellationToken.IsCancellationRequested)
					throw;

				var reason = e is OperationCanceledException ? $"timed out after {attemptMs} ms" : e.Message;
				(failures ??= new()).Add($"{address}: {reason}");
			}
		}

		throw Fail($"Connection to {host}:{port} failed ({(failures is null ? "no address tried" : string.Join("; ", failures))}).");
	}

	private static PgOperationalError Fail(string message) => new(message, "08001", "FATAL", "", "");

}