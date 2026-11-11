using System.Buffers;
using System.Security.Cryptography;

namespace Ring.PostgreSQL.Helpers;

/// <summary>
/// A <see cref="MemoryPool{T}"/> over <see cref="ArrayPool{T}.Shared"/> that zeroes a buffer at the moment its owner is disposed,
/// i.e. when the <see cref="System.IO.Pipelines.PipeWriter"/> that rented it gives it back.
/// <para>
/// This is how secrets (cleartext/MD5 password, SCRAM client proof) leave the pipe buffers without the caller ever touching memory
/// after <c>Advance</c>: past that point the buffer belongs to the writer, which may still be sending it, keep it as its tail segment
/// or hand it back to a pool. Zeroing at release time is the only point where the pool - the real owner - knows nobody uses it anymore.
/// </para>
/// <para>
/// Used for the writer only (the reader never holds secrets). Cost: one memset per released buffer (a few hundred ns for 16 KB).
/// </para>
/// </summary>
internal sealed class ScrubbingMemoryPool : MemoryPool<byte>
{
	private const int DefaultBufferSize = 4096; // same default as MemoryPool<byte>.Shared when no size is requested
	public static readonly ScrubbingMemoryPool Instance = new();
	public override int MaxBufferSize => Array.MaxLength;

	public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
	{
		if (minBufferSize == -1) minBufferSize = DefaultBufferSize;
		ArgumentOutOfRangeException.ThrowIfNegative(minBufferSize);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(minBufferSize, MaxBufferSize);
		return new Owner(ArrayPool<byte>.Shared.Rent(minBufferSize));
	}

	// Process-wide singleton over the shared ArrayPool: nothing to release.
	protected override void Dispose(bool disposing) {}

	private sealed class Owner : IMemoryOwner<byte>
	{
		private byte[]? _array;

		public Owner(byte[] array) => _array = array;
		public Memory<byte> Memory => _array ?? throw new ObjectDisposedException(nameof(ScrubbingMemoryPool));
		public void Dispose()
		{
			var array = Interlocked.Exchange(ref _array, null);
			if (array is null) return;
			CryptographicOperations.ZeroMemory(array); // not elided by the JIT, unlike Array.Clear on a soon-dead array
			ArrayPool<byte>.Shared.Return(array);
		}
	}

}
