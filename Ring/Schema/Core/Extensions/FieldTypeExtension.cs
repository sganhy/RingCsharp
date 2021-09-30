using Ring.Data;
using Ring.Schema.Enums;
using System;

namespace Ring.Schema.Core.Extensions
{
    internal static class FieldTypeExtension
    {
        public static int GetId(this FieldType fieldType) =>
            fieldType != FieldType.NotDefined ? (int)fieldType : Constants.NotDefinedFieldTypeId;

        /// <summary>
        /// 
        /// </summary>
        public static string GetDefault(this FieldType fieldType, DatabaseProvider provider, string value)
        {
            switch (fieldType)
            {
                case FieldType.ShortDateTime:
                case FieldType.DateTime:
                case FieldType.LongDateTime: return Constants.FieldDefaultDate;
                case FieldType.Long:
                case FieldType.Int:
                case FieldType.Short:
                case FieldType.Byte:
                case FieldType.Float:
                case FieldType.Double:
                    return Constants.DefaultNumericValue;
                case FieldType.Boolean:
                    return string.IsNullOrEmpty(value) ? bool.FalseString : value;
                case FieldType.String:
                    break;
                case FieldType.Array:
                    break;
                case FieldType.NotDefined:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return value;
        }

    }
}
