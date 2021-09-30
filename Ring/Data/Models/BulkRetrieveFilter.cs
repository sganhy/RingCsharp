using Ring.Schema.Models;

namespace Ring.Data.Models
{
    internal sealed class BulkRetrieveFilter
    {
        internal readonly Field Field;
        internal readonly OperationType Operation;
        internal readonly string Operand;
        internal readonly string[] Operands;
        internal readonly bool CaseSensitiveSearch; // case sensitif search ??

        /// <summary>
        /// Ctor 
        /// </summary>
        public BulkRetrieveFilter(Field field, OperationType operation, string operand, bool caseSensitive)
        {
            Field = field;
            Operation = operation;
            Operand = operand;
            Operands = null;
            CaseSensitiveSearch = caseSensitive;
        }
        public BulkRetrieveFilter(Field field, OperationType operation, string[] operands) : this(field, operation, null, true)
        {
            Operands = operands;
        }


    }
}
