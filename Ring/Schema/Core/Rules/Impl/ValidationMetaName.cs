using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Core.Extensions;
using System.Collections.Generic;

namespace Ring.Schema.Core.Rules.Impl
{
    /// <summary>
    /// Name validation for metadata Object
    /// </summary>
    internal sealed class ValidationMetaName : IValidationRule<MetaData>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public ValidationResult Validate(MetaData[] source)
        {
            var result = new ValidationResult();
            result.Merge(CheckName(source));
            result.Merge(CheckUniqueName(source));
            return result;
        }

        /// <summary>
        /// Check if name consist of any combination of letters(A to Z a to z), decimal digits(0 to 9) or underscore (_).
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ValidationResult CheckName(MetaData[] source)
        {
            var result = new ValidationResult();

            for (var i = 0; i < source.Length; ++i)
            {
                var meta = source[i];
                var metaType = meta.GetEntityType();

                // 1) Is name empty ? (schema name can be null) 
                if (string.IsNullOrWhiteSpace(meta.Name) && metaType != EntityType.Schema)
                {
                    result.AddItem(Constants.MetadataEmptyName.Id, meta.LineNumber,
                        string.Format(Constants.MetadataEmptyName.Name, metaType.ToString()),
                        string.Format(Constants.MetadataEmptyName.Description, metaType.ToString()),
                        Constants.MetadataEmptyName.Level);
                }
                else
                {
                    // 2) Is name valid ? TODO manage system sequence & language validation
                    if (metaType != EntityType.Schema && metaType != EntityType.Language &&
                        metaType != EntityType.Sequence && !IsValidName(meta.Name))
                    {
                        result.AddItem(Constants.MetadataInvalidName.Id, meta.LineNumber,
                            string.Format(Constants.MetadataInvalidName.Name, metaType.ToString()),
                            string.Format(Constants.MetadataInvalidName.Description, metaType.ToString(), meta.Name),
                            Constants.MetadataInvalidName.Level);
                    }

                    // 3) Is name length is less or equal than 30 (except objects with prefix tables & fields)
                    if (metaType != EntityType.Table &&
                        metaType != EntityType.Field &&
                        meta.Name?.Length > Constants.MaxSizeObjectName)
                    {
                        result.AddItem(Constants.MetadataNameIsTooLong.Id, meta.LineNumber,
                            string.Format(Constants.MetadataNameIsTooLong.Name, metaType.ToString()),
                            string.Format(Constants.MetadataNameIsTooLong.Description, metaType.ToString(), meta.Name,
                                Constants.MaxSizeObjectName.ToString()),
                            Constants.MetadataNameIsTooLong.Level);
                    }

                    // 4) Is name length is less or equal than 28 (except objects with prefix tables & fields)
                    if ((metaType == EntityType.Table || metaType == EntityType.Field) &&
                        meta.Name?.Length > Constants.MaxSizeObjectNameWithPrefix)
                    {
                        result.AddItem(Constants.MetadataNameIsTooLong.Id, meta.LineNumber,
                            string.Format(Constants.MetadataNameIsTooLong.Name, metaType.ToString()),
                            string.Format(Constants.MetadataNameIsTooLong.Description, metaType.ToString(), meta.Name,
                                Constants.MaxSizeObjectNameWithPrefix.ToString()),
                            Constants.MetadataNameIsTooLong.Level);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Check for relation and field if there unique for the same table
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ValidationResult CheckUniqueName(MetaData[] source)
        {
            var result = new ValidationResult();
            var entityDico = new SortedDictionary<string, SortedDictionary<string, bool>>();  // <table.id, <Entity.name.ToUpper(), false>>
            var tableDico = new SortedDictionary<string, string>();

            // (1) build table dictionary
            for (var i = 0; i < source.Length; ++i)
                if (source[i].GetEntityType() == EntityType.Table && !tableDico.ContainsKey(source[i].Id))
                    tableDico.Add(source[i].Id, source[i].Name);

            // (2) build dictionary entity &  check 
            for (var i = 0; i < source.Length; ++i)
            {
                var meta = source[i];
                var metaType = meta.GetEntityType();
                if (metaType == EntityType.Field || metaType == EntityType.Relation)
                {
                    if (!entityDico.ContainsKey(meta.RefId)) entityDico.Add(meta.RefId, new SortedDictionary<string, bool>());

                    // @"Duplicate field {0} for table {1}",
                    if (entityDico[source[i].RefId].ContainsKey(meta.Name.ToUpper()))
                        result.AddItem(Constants.MetadataDuplicateName.Id, meta.LineNumber,
                            Constants.MetadataDuplicateName.Name,
                            string.Format(Constants.MetadataDuplicateName.Description, meta.Name,
                                tableDico.TryGetValue(meta.RefId)),
                            Constants.MetadataDuplicateName.Level);
                    else entityDico[source[i].RefId].Add(meta.Name.ToUpper(), false);
                }
            }
            return result;
        }

        /// <summary>
        /// Check if A name consist of any combination of letters(A to Z a to z), decimal digits(0 to 9) or underscore (_).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool IsValidName(string name)
        {
            if (name == null) return false;
            if (name.Length <= 0) return false;
            char c;
            var strSize = name.Length;
            for (var i = 0; i < strSize; ++i)
            {
                c = name[i];
                if (!((c >= 'a' && c <= 'z') ||
                      (c >= 'A' && c <= 'Z') ||
                      (c >= '0' && c <= '9') ||
                       c == '_')) return false;
            }
            return true;
        }

    }
}