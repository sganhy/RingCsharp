using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Schema.Models;
using Index = Ring.Schema.Models.Index;

namespace Ring.Data.Models;

public readonly struct AlterQuery : IEquatable<AlterQuery>
{
	internal readonly int Id;
	internal readonly Table Table;
	internal readonly AlterQueryType Type;
	internal readonly Column? Column;
	internal readonly Constraint? Constraint;
	internal readonly Index? Index;
	internal readonly TableSpace? TableSpace;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal AlterQuery(int id, Table table, AlterQueryType type, in Column? column, Constraint? constraint, Index? index, TableSpace? tableSpace)
	{
		Id = id;
		Table = table;
		Type = type;
		Column = column;
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
