using Ring.Schema.Models;

namespace Ring.Data.Models;

internal struct BulkRetrieveFilter
{
    internal readonly Field Field;
    internal readonly Operator Operator;
    internal string? Operand;
    internal string[]? Operands;
    internal readonly bool CaseSensitiveSearch; // case sensitif search ??

    public BulkRetrieveFilter(Field field, Operator operation, bool caseSensitiveSearch=false)
    {
        Field = field;
        Operator = operation;
        Operand = null;
        Operands = null;
        CaseSensitiveSearch = caseSensitiveSearch;
    }
}
