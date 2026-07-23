using System.Runtime.CompilerServices;

namespace Ring.PostgreSQL.Helpers;

internal static class ArrayExtensions
{
	/// <summary>
	///     Scans an ErrorResponse/NoticeResponse body for just the SQLSTATE
	///     ('C') field and returns it as a slice of <paramref name="body"/> -
	///     no string allocation, no copy, unlike <see cref="ParseErrorResponse"/>
	///     which decodes every field into its own string. Intended for cheap
	///     "is this a specific error code" checks on a hot path (e.g. treating
	///     42P07/duplicate_table as a no-op) before paying for the full parse.
	///     Returns an empty span if the field is absent.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ReadOnlySpan<byte> ReadSqlStateBytes(this byte[] body)
	{
		// Code size: 73 (0x49)
		var offset = 0;
		while (offset < body.Length && body[offset] != 0)
		{
			var field = body[offset++];
			var start = offset;
			while (offset < body.Length && body[offset] != 0) offset++;
			if (field == (byte)'C')
				return body.AsSpan(start, offset - start);
			offset++; // skip this field's null terminator
		}
		return ReadOnlySpan<byte>.Empty;
	}
}
