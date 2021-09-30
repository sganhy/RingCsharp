using Ring.Schema.Models;

namespace Ring.Schema.Core.Rules.Impl
{
    internal sealed class ValidationTableMetaKey : IValidationRule<Table>
    {
        public ValidationResult Validate(Table[] source)
        {
            var result = new ValidationResult();
            result.Merge(CheckUniqueKey(source));
            return result;
        }

        /// <summary>
        /// Check meta data unique key
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ValidationResult CheckUniqueKey(Table[] source)
        {
            if (source == null) return null;
            var result = new ValidationResult();
            //var dico = new Dictionary<decimal, Table>();
            /*
            for (var i=0; i<source.Length; ++i)
            {
                var table = source[i];
                var newKey = GetMetaDataKey(table);
                if (!dico.ContainsKey(newKey)) dico.Add(newKey, table);
            }
            */
            return result;
        }

        /*
        private static decimal GetMetaDataKey(Table table)
        {
            // key: id,reference_id,schema_id,object_type

            return 0; 
        }
        */
    }
}
