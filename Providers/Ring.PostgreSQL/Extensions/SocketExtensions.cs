using System.Net.Sockets;

namespace Ring.PostgreSQL.Extensions;

internal static class SocketExtensions
{
	private const int KeepAliveTimeSeconds = 60;
	private const int KeepAliveIntervalSeconds = 10;
	private const int KeepAliveRetryCount = 3;

	// Detects silently dropped connections (cable pulled, firewall idle timeout, crashed server host).
	public static void ConfigureKeepAlive(this Socket socket)
	{
		try
		{
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
			socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, KeepAliveTimeSeconds);
			socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, KeepAliveIntervalSeconds);
			socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, KeepAliveRetryCount);
		}
		catch (SocketException) { }
		catch (PlatformNotSupportedException) { }
		catch (ArgumentException) { }
	}
}
