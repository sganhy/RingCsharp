using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

/// <summary>
/// 	Logical index
/// </summary>
internal sealed class Index : BaseEntity, IEquatable<Index>
{
	internal readonly bool Bitmap;
	internal readonly Column[] Columns;
	internal readonly bool Unique;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Index(int id, string name, string? description, Column[] columns, bool unique, bool bitmap, bool active, bool baseline) : base(id, name, description, baseline, active)
	{
		Unique = unique;
		Columns = columns;
		Bitmap = bitmap;
	}

	public static bool operator ==(Index left, Index right) => left.Equals(right);
	public static bool operator !=(Index left, Index right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is Index index && Equals(index);
	public bool Equals(Index? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();
}