using Ring.Schema.Enums;

namespace Ring.Schema.Models;

/// <summary>
/// 	Logical field sourceType
/// </summary>
internal sealed class Field : BaseEntity, IColumn
{
	internal readonly SearchableType SearchableType;
	internal readonly string? DefaultValue;
	internal readonly bool Multilingual;
	internal readonly bool NotNull;
	internal readonly int Size;
	internal readonly FieldType Type;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Field(int id, string name, string physicalName, string? description, FieldType type, int size, string? defaultValue,
		SearchableType searchableType, bool baseline, bool notNull, bool multilingual, bool active)
		: base(id, name, physicalName, description, baseline, active)
	{
		Type = type;
		Size = size;
		DefaultValue = defaultValue;
		NotNull = notNull;
		SearchableType = searchableType;
		Multilingual = multilingual;
	}

	/// <summary>
	/// 	Implement IColumn
	/// </summary>
	int IColumn.Id => Id;
	FieldType IColumn.FieldType => Type;
	RelationType IColumn.RelationType => RelationType.Undefined;
	EntityType IColumn.Type => EntityType.Field;
	string IColumn.Name => Name;
	string IColumn.PhysicalName => PhysicalName;

#if DEBUG
	public override string ToString() => $"{Id} - {Name} ({Type})";
#endif
}