namespace Ring.Schema.Models
{
    internal sealed class Node : BaseEntity
    {
        internal Node(int id, string name, string description, bool enabled, bool baseline)
            : base(id, name, description, enabled, baseline)
        {
        }


    }
}
