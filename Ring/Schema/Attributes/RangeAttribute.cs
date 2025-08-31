namespace Ring.Schema.Attributes;

[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
internal sealed class RangeAttribute : Attribute
{
	internal int MininmumId { get; }
	internal int MaximumId { get; }

	internal RangeAttribute(int minId, int maxId)
	{
		MininmumId = minId;
		MaximumId = maxId;
	}
}
