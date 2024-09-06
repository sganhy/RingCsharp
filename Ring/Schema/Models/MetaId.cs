using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class MetaId
{
	internal readonly int Id;
	internal EntityType ObjectType;
	internal readonly long Value;

	internal MetaId(int id, EntityType objectType, long value)
	{
		Id = id;
		ObjectType = objectType;
		Value = value;
	}
}