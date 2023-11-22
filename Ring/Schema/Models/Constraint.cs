using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Constraint
{
    internal readonly ConstraintType Type;
    internal readonly Table ToTable;
    internal readonly Field ToField;
    internal readonly Relation ToRelation;

    internal Constraint(ConstraintType type, Table table, Field field, Relation relation)
    {
        Type = type;
        ToField = field; 
        ToTable = table;
        ToRelation = relation;
    }
}
