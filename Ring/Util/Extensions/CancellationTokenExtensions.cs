namespace Ring.Util.Extensions;

internal static class CancellationTokenExtensions
{
	// Applies CommandTimeout. Reuses one CancellationTokenSource when the caller passes no token;
	// a linked source is only created when both a caller token and a timeout exist.
	internal static CancellationToken AcquireToken(this in CancellationToken external, out CancellationTokenSource? linked, in TimeSpan commandTimeout, CancellationTokenSource? commandCts)
	{
		// Code size: 102 (0x66)
		linked = null;
		var timeout = commandTimeout;
		if (timeout <= TimeSpan.Zero) return external;
		if (external.CanBeCanceled)
		{
			linked = CancellationTokenSource.CreateLinkedTokenSource(external);
			linked.CancelAfter(timeout);
			return linked.Token;
		}
		if (commandCts is null)
		{
			var cts = new CancellationTokenSource();
			cts.CancelAfter(timeout);
			return cts.Token;
		}
		else
		{
			commandCts.CancelAfter(timeout);
			return commandCts.Token;
		}
	}
}
