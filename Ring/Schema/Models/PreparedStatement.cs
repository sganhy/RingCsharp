namespace Ring.Schema.Models
{
    internal sealed class PreparedStatement : BaseEntity
    {
        public PreparedStatement(int id, string name, string description, bool active, bool baseline)
            : base(id, name, description, active, baseline)
        {
        }

    }
}
