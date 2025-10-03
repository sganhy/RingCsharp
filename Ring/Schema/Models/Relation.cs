using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

/// <summary>
/// 	Logical relation
/// </summary>
internal sealed class Relation : BaseEntity, IEquatable<Relation>
{
	internal Relation InverseRelation { get; private set; } // assigned after initialization
	internal readonly bool HasConstraint;
	internal readonly bool NotNull;
	internal readonly Table ToTable;
	internal readonly RelationType Type;
	internal readonly FieldType FieldType;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Relation(int id, string name, string? description, RelationType type, Table toObject, FieldType fieldType, 
		bool notnull, bool constraint, bool baseline, bool active) : base(id, name, description, baseline, active)
	{
		Type = type;
		ToTable = toObject;
		HasConstraint = constraint;
		NotNull = notnull;
		InverseRelation = this;
		FieldType = fieldType;
	}

	/// <summary>
	/// 	Assign only once the property 
	/// </summary>
	internal void SetInverseRelation(Relation relation) => InverseRelation = ReferenceEquals(InverseRelation,this) ? relation : InverseRelation;
	public static bool operator ==(Relation left, Relation right) => left.Equals(right);
	public static bool operator !=(Relation left, Relation right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is Relation relation && Equals(relation);
	public bool Equals(Relation? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();
}