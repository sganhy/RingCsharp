using Ring.Schema.Enums;
using System.Drawing;

namespace Ring.Schema.Models;

/// <summary>
/// 	Logical relation
/// </summary>
internal sealed class Relation : BaseEntity
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
	internal Relation(int id, string name, string? description, RelationType type, Table toObject, int recordIndex,
		FieldType fieldType, bool notnull, bool constraint, bool baseline, bool active)
		: base(id, name, description, baseline, active)
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

}