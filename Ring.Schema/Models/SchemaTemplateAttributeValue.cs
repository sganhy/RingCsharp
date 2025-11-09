namespace Ring.Schema.Models;

internal sealed class SchemaTemplateAttributeValue
{
	internal readonly int Id;
	internal readonly string Value;

	internal SchemaTemplateAttributeValue(int id, string value)
	{
		Id = id;
		Value = value;
	}

#if DEBUG
	public override string ToString() => $"{Id} ({Value})";
#endif

}
