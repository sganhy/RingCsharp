using Ring.Schema.Enums;

namespace Ring.Util.Models;

internal sealed class SchemaTemplateItem
{
	internal readonly EntityType EntityType;
	internal readonly string Tag;
	internal readonly string ParentTag;
	internal readonly string ChildDescriptionTag;
	internal readonly string ChildIndexColumnTag;
	internal readonly SchemaTemplateAttribute[] Attributes;

	internal SchemaTemplateItem(EntityType entityType, string startTag, string parentTag, string childDescriptionTag, string childIndexColumnTag, SchemaTemplateAttribute[] attributes)
	{
		EntityType = entityType;
		Tag = startTag;
		ParentTag = parentTag;
		ChildDescriptionTag = childDescriptionTag;
		Attributes = attributes;
		ChildIndexColumnTag = childIndexColumnTag;
	}

#if DEBUG
	public override string ToString() => $"{Tag} ({EntityType})";
#endif
}
