using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class Relation : BaseEntity, IColumn
{
	internal Relation InverseRelation { get; private set; } // assigned after initialization
	internal readonly bool HasConstraint;                   // foreign key constraint should be added
	internal readonly bool NotNull;                         // foreign key constraint should be added
	internal readonly Table ToTable;
	internal readonly RelationType Type;
	internal int RecordIndex { get; private set; }          // index of relationship into record._data by default -1

	/// <summary>
	///     Ctor
	/// </summary>
	internal Relation(int id, string name, string? description, RelationType type, Table toObject, int recordIndex,
		bool notnull, bool constraint,bool baseline, bool active)
		: base(id, name, description, active, baseline)
	{
		Type = type;
		ToTable = toObject;
		HasConstraint = constraint;
		NotNull = notnull;
		InverseRelation = this;
		RecordIndex = recordIndex;
	}

	/// <summary>
	///     assign only once the property 
	/// </summary>
	internal void SetInverseRelation(Relation relation) => 
		InverseRelation = ReferenceEquals(InverseRelation,this) ? relation : InverseRelation;

	internal void SetRecordIndex(int index) => RecordIndex = index;

	/// <summary>
	///     Implement IColumn
	/// </summary>
	int IColumn.Id => Id;
	FieldType IColumn.FieldType => ToTable.GetPrimaryKey()?.Type ?? FieldType.Undefined;
	string IColumn.Name => Name;
	RelationType IColumn.RelationType => Type;
	EntityType IColumn.Type => EntityType.Relation;
}