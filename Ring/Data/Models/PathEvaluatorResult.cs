using Ring.Schema.Enums;

namespace Ring.Data.Models
{
    internal class PathEvaluatorResult
    {
        public string Value;
        public readonly FieldType Type;

        public PathEvaluatorResult(FieldType entityType, string value)
        {
            Value = value;
            Type = entityType;
        }
    }
}
