using System.Text;
using Ring.Schema.Models;

namespace Ring.Data.Models
{
    internal sealed class PathEvaluatorFilter
    {
        internal readonly Field Field;
        internal readonly OperationType Operation;
        internal readonly string Operand;

        /// <summary>
        /// Ctor
        /// </summary>
        public PathEvaluatorFilter(Field field, OperationType operationType, string operand)
        {
            //
            //eg. 
            //  filter   ==> (T2.objid != 0)
            //  fullPath ==> case_currq2queue:queue_supvr2user(T2.objid != 0):node_id
            //
            Field = field;
            Operation = operationType;
            Operand = operand;
        }


#if DEBUG

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Field != null) sb.Append(Field.Name);
            sb.Append(' ');
            sb.Append(Operation.ToString());
            sb.Append(' ');
            if (Operand != null) sb.Append(Operand);
            return sb.ToString();
        }

#endif

    }

}
