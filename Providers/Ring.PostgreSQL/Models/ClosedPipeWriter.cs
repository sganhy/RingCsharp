using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.IO.Pipelines;

namespace Ring.PostgreSQL.Models;

/// <summary>
/// Stateless stand-in for a writer on a connection that is not open (see <see cref="ClosedPipeReader"/>).
/// </summary>
internal sealed class ClosedPipeWriter : PipeWriter
{
	internal static readonly ClosedPipeWriter Instance = new();
	private ClosedPipeWriter() { }
	public override void Advance(int bytes) => ThrowConnectionNotOpen();
	public override Memory<byte> GetMemory(int sizeHint = 0) { ThrowConnectionNotOpen(); return new Memory<byte>(); }
	public override Span<byte> GetSpan(int sizeHint = 0) { ThrowConnectionNotOpen(); return Array.Empty<byte>(); }
	public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default) { ThrowConnectionNotOpen(); return new ValueTask<FlushResult>(new FlushResult()); }
	public override void CancelPendingFlush() { }
	public override void Complete(Exception? exception = null) { }
	private static void ThrowConnectionNotOpen() => throw new InvalidOperationException(ResourceHelper.GetMessage(ResourceType.ConnectionNotOpen));
}