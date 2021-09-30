using Ring.Schema.Enums;

namespace Ring.Schema.Models
{
    internal sealed class Relation : BaseEntity
    {
        internal readonly string InverseRelationName;
        internal readonly string MtmTable;
	    internal readonly bool Constraint; // foreign key constraint should be added
	    internal readonly bool NotNull; // foreign key constraint should be added
		internal readonly Table To;
        internal readonly RelationType Type;

        /// <summary>
        ///     Ctor
        /// </summary>
        internal Relation(int id, string name, string description, RelationType type, Table toObject, string mtm,
            string inverseRelationName,bool notnull, bool constraint,bool baseline, bool active)
            : base(id, name, description, active, baseline)
        {
            Type = type;
            To = toObject;
            MtmTable = mtm;
	        Constraint = constraint;
	        NotNull = notnull;
			InverseRelationName = inverseRelationName;
        }
    }
}