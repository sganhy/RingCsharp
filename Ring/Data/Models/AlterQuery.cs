using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Schema.Models;
using Index = Ring.Schema.Models.Index;

namespace Ring.Data.Models;

public readonly struct AlterQuery : IEquatable<AlterQuery>
{
	// Table(8) + Constraint(8) + Index(8) + TableSpace(8) + Id(4) + ColumnIndex(4) + Type(1) + [7 trailing padding] = 48 bytes
	// 8-byte reference types (32 bytes total)
	internal readonly Table Table;
	internal readonly Constraint? Constraint;
	internal readonly Index? Index;
	internal readonly TableSpace? TableSpace;

	// 4-byte integers (8 bytes total)
	internal readonly int Id;
	internal readonly int ColumnIndex; // -1 = no column

	// 1-byte enum (1 byte payload + 7 bytes trailing padding for 8-byte alignment)
	internal readonly AlterQueryType Type;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal AlterQuery(int id, Table table, AlterQueryType type, int columnIndex, Constraint? constraint, Index? index, TableSpace? tableSpace)
	{
		Id = id;
		Table = table;
		Type = type;
		ColumnIndex = columnIndex;
		Constraint = constraint;
		Index = index;
		TableSpace = tableSpace;
	}

	public static bool operator ==(AlterQuery left, AlterQuery right) => left.Equals(right);
	public static bool operator !=(AlterQuery left, AlterQuery right) => !left.Equals(right);
	public override readonly bool Equals(object? obj) => obj is AlterQuery alterQuery && Equals(alterQuery);
	public readonly bool Equals(AlterQuery other) => this.IsEquivalentTo(other);
	public override readonly int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Id} - {Type}";
#endif

}
