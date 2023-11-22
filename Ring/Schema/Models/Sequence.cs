namespace Ring.Schema.Models;
internal sealed class Sequence : BaseEntity
{
    internal readonly int SchemaId;
    internal readonly long MaxValue;
    internal readonly CacheId Value;

    internal Sequence(int id, string name, string description, int schemaId, long maxValue,
        bool baseline, bool active)
        : base(id, name, description, active, baseline)
    {
        SchemaId = schemaId;
        Value = new CacheId();
        MaxValue = maxValue;

    }
}