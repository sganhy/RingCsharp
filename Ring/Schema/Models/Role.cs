namespace Ring.Schema.Models
{
    internal sealed class Role : BaseEntity
    {
        internal readonly bool Administrator;

        /// <summary>
        ///     Ctor
        /// </summary>
        public Role(int id, string name, string description, bool active, bool baseline, bool administrator)
            : base(id, name, description, active, baseline)
        {
            Administrator = administrator;
        }
    }
}