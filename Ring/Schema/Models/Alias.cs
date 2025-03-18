using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Alias : BaseEntity
{
	internal readonly int TargetEntityId;
	internal readonly EntityType TargetEntityType;
	internal readonly AliasType Type;
	internal readonly string? Value;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Alias(int id, string name, string physicalName, string? description, AliasType type, EntityType entityType, int entityId,
		string? value, bool baseline, bool active) : base(id, name, description, baseline, active)
	{
		Type = type;
		TargetEntityType = entityType;
		TargetEntityId = entityId;
		Value = value;
	}
}
