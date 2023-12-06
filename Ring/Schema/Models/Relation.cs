using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Relation : BaseEntity
{
	internal Relation InverseRelation { get; private set; } // assigned after initialization
	internal readonly bool HasConstraint;                   // foreign key constraint should be added
	internal readonly bool NotNull;                         // foreign key constraint should be added
	internal readonly Table ToTable;
	internal readonly RelationType Type;

	/// <summary>
	///     Ctor
	/// </summary>
	internal Relation(int id, string name, string? description, RelationType type, Table toObject,
		bool notnull, bool constraint,bool baseline, bool active)
		: base(id, name, description, active, baseline)
	{
		Type = type;
		ToTable = toObject;
		HasConstraint = constraint;
		NotNull = notnull;
		InverseRelation = this;
	}

	// assign only once the property
	internal void SetInverseRelation(Relation relation) => 
		InverseRelation = ReferenceEquals(InverseRelation,this) ? relation : InverseRelation;

}