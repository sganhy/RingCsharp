using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class SchemaTemplateItem
{
	internal readonly int EntityTypeId;
	internal readonly EntityType EntityType;
	internal readonly int Depth;
	internal readonly string Tag;
	internal readonly string ParentTag;
	internal readonly string ChildDescriptionTag;
	internal readonly string ChildIndexColumnTag;
	internal readonly SchemaTemplateAttribute[] Attributes;

	internal SchemaTemplateItem(EntityType entityType, string startTag, string parentTag, string childDescriptionTag, string childIndexColumnTag, int depth, SchemaTemplateAttribute[] attributes)
	{
		EntityTypeId = (byte)entityType;
		EntityType = entityType;
		Tag = startTag;
		Depth = depth;
		ParentTag = parentTag;
		ChildDescriptionTag = childDescriptionTag;
		Attributes = attributes;
		ChildIndexColumnTag = childIndexColumnTag;
	}

#if DEBUG
	public override string ToString() => $"{Tag} ({EntityType})";
#endif
}
