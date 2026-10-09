using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.IO.Pipelines;

namespace Ring.PostgreSQL.Models;

/// <summary>
/// Stateless stand-in for a reader on a connection that is not open. Every read or advance fails with a clear message;
/// Complete and CancelPendingRead are no-ops so cleanup code can call them unconditionally.
/// </summary>
internal sealed class ClosedPipeReader : PipeReader
{
	internal static readonly ClosedPipeReader Instance = new();
	private ClosedPipeReader() {}
	public override bool TryRead(out ReadResult result) { ThrowConnectionNotOpen(); result = default; return false; }
	public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default) { ThrowConnectionNotOpen(); return new ValueTask<ReadResult>(new ReadResult()); }
	public override void AdvanceTo(SequencePosition consumed) => ThrowConnectionNotOpen();
	public override void AdvanceTo(SequencePosition consumed, SequencePosition examined) =>	ThrowConnectionNotOpen();
	public override void CancelPendingRead() {}
	public override void Complete(Exception? exception = null) {}
	private static void ThrowConnectionNotOpen() => throw new InvalidOperationException(ResourceHelper.GetMessage(ResourceType.ConnectionNotOpen));
}
