using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Core.Rules.Impl
{
    internal sealed class ValidationMetaDataType : IValidationRule<MetaData>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public ValidationResult Validate(MetaData[] source)
        {
            var result = new ValidationResult();
            result.Merge(CheckDataType(source));
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ValidationResult CheckDataType(MetaData[] source)
        {
            var result = new ValidationResult();

            for (var i = 0; i < source.Length; ++i)
            {
                var meta = source[i];
                var metaType = meta.GetEntityType();

                // 1) Is datatype not found - invalid dataType
                if (metaType == EntityType.Field && meta.DataType == Constants.NotDefinedFieldTypeId)
                {
                    result.AddItem(Constants.MetadataInvalidType.Id, meta.LineNumber,
                        string.Format(Constants.MetadataInvalidType.Name, metaType.ToString()),
                        string.Format(Constants.MetadataInvalidType.Description, metaType.ToString(), meta.Value),
                            Constants.MetadataInvalidType.Level);
                }

                // 2 relation dataType - check if table exist
                if (metaType == EntityType.Relation && meta.DataType == Constants.TableIdNotFound)
                {
                    result.AddItem(Constants.MetadataInvalidTo.Id, meta.LineNumber,
                       string.Format(Constants.MetadataInvalidTo.Name, metaType.ToString()),
                       string.Format(Constants.MetadataInvalidTo.Description, meta.Value),
                           Constants.MetadataInvalidTo.Level);
                }

            }
            return result;
        }

    }
}
