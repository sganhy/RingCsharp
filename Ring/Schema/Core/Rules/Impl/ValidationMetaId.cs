using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Core.Extensions;
using System.Collections.Generic;

namespace Ring.Schema.Core.Rules.Impl
{
    internal sealed class ValidationMetaId : IValidationRule<MetaData>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public ValidationResult Validate(MetaData[] source)
        {
            var result = new ValidationResult();

            result.Merge(ValidId(source));

            // if all id are valid - check if there are unique 
            if (!result.IsBlockingDefect) result.Merge(TableIdUnique(source));

            return result;
        }

        /// <summary>
        /// check if Id are valid  
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ValidationResult ValidId(MetaData[] source)
        {
            var result = new ValidationResult();

            for (var i = 0; i < source.Length; ++i)
            {
                var meta = source[i];
                var metaType = meta.GetEntityType();

                // 1) Is ID empty ? (schema name can be null) 
                if (string.IsNullOrWhiteSpace(meta.Id) && metaType == EntityType.Table)
                {
                    result.AddItem(Constants.MetadataEmptyId.Id, meta.LineNumber,
                        string.Format(Constants.MetadataEmptyId.Name, metaType.ToString()),
                        string.Format(Constants.MetadataEmptyId.Description, metaType.ToString()),
                            Constants.MetadataEmptyId.Level);
                }
                else
                {
                    // 2) Is Id valid ?
                    if (metaType == EntityType.Table && !IsNumber(meta.Id))
                    {
                        result.AddItem(Constants.MetadataInvalidId.Id, meta.LineNumber, Constants.MetadataInvalidId.Name,
                                    string.Format(Constants.MetadataInvalidId.Description, metaType.ToString(), meta.Name),
                                    Constants.MetadataInvalidId.Level);
                    }
                    else
                    {
                        int tempId;
                        // 3) Is Id is out of range ?
                        if (metaType == EntityType.Table && !int.TryParse(meta.Id, out tempId))
                        {
                            result.AddItem(Constants.MetadataOutOfRangeId.Id, meta.LineNumber, Constants.MetadataOutOfRangeId.Name,
                                    string.Format(Constants.MetadataOutOfRangeId.Description, metaType.ToString(), meta.Name),
                                    Constants.MetadataOutOfRangeId.Level);
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Check if table id are unique ?
        /// </summary>
        /// <returns></returns>
        private static ValidationResult TableIdUnique(MetaData[] source)
        {
            var result = new ValidationResult();
            var dico = new SortedDictionary<string, List<MetaData>>();  // <table.id, List<MetaData>>

            // first pass build dictionary (dico)
            for (var i = 0; i < source.Length; ++i)
            {
                var meta = source[i];
                if (meta.GetEntityType() == EntityType.Table)
                {
                    if (dico.ContainsKey(meta.Id)) dico[meta.Id].Add(meta);
                    else dico.Add(meta.Id, new List<MetaData> { meta });
                }
            }
            var dicoArr = dico.ToArray();
            for (var i = 0; i < dicoArr.Length; ++i)
            {
                var keyValue = dicoArr[i];
                // detect duplicate 
                if (keyValue.Value.Count > 1)
                {
                    var message = keyValue.Value[0].Name;
                    for (var j = 1; j < keyValue.Value.Count; ++j) message += Constants.MsgCharSeparator + keyValue.Value[j].Name;
                    result.AddItem(Constants.MetadataDuplicateId.Id, keyValue.Value[0].LineNumber,
                        string.Format(Constants.MetadataDuplicateId.Name, keyValue.Key),
                        string.Format(Constants.MetadataDuplicateId.Description, keyValue.Key, message),
                        Constants.MetadataOutOfRangeId.Level);
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static bool IsNumber(string id)
        {
            if (id == null) return false;
            if (id.Length <= 0) return false;
            for (var i = 0; i < id.Length; ++i)
            {
                var c = id[i];
                if (!(c >= '0' && c <= '9')) return false;
            }
            return true;
        }

    }
}
