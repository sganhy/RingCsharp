using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Core.Extensions;
using System.Collections.Generic;

namespace Ring.Schema.Core.Rules.Impl
{
    internal sealed class ValidationMetaIndex : IValidationRule<MetaData>
    {
        /// <summary>
        /// 
        /// </summary>
        public ValidationResult Validate(MetaData[] source)
        {
            var result = new ValidationResult();
            result.Merge(CheckUniqueDefinition(source));
            result.Merge(CheckFieldDefinition(source));
            return result;
        }


        /// <summary>
        /// Detect duplicate index definition ()
        /// </summary>
        private static ValidationResult CheckUniqueDefinition(MetaData[] source)
        {
            var result = new ValidationResult();
            var tableDico = new SortedDictionary<string, string>();
            var entityDico = new SortedDictionary<string, SortedDictionary<string, bool>>(); // <table.id, <Entity.name.ToUpper(), false>>

            // (1) build table dictionary
            for (var i = 0; i < source.Length; ++i)
                if (source[i].GetEntityType() == EntityType.Table && !tableDico.ContainsKey(source[i].Id))
                    tableDico.Add(source[i].Id, source[i].Name);

            // (2) check duplicate index definition
            for (var i = 0; i < source.Length; ++i)
            {
                var meta = source[i];
                var metaType = meta.GetEntityType();

                if (metaType == EntityType.Index)
                {
                    if (!entityDico.ContainsKey(meta.RefId)) entityDico.Add(meta.RefId, new SortedDictionary<string, bool>());
                    var indexDefinition = meta.Value.ToUpper() + Constants.IndexFieldSeparator + meta.Flags.ToString();
                    if (entityDico[source[i].RefId].ContainsKey(indexDefinition))
                        result.AddItem(Constants.MetadataDuplicateIndexDef.Id, meta.LineNumber,
                            Constants.MetadataDuplicateIndexDef.Name,
                            string.Format(Constants.MetadataDuplicateIndexDef.Description, meta.Name,
                                tableDico.TryGetValue(meta.RefId)),
                            Constants.MetadataDuplicateName.Level);
                    else entityDico[source[i].RefId].Add(indexDefinition, false);
                }
            }

            return result;
        }

        /// <summary>
        /// Check if indexes reference existing fields 
        /// </summary>
        private static ValidationResult CheckFieldDefinition(MetaData[] source)
        {
            var result = new ValidationResult();
            var entityDico = new Dictionary<string, HashSet<string>>();  // <table, HashSet<field.ToUpper()>>
            var tableDico = new Dictionary<string, string>(); // <table.Id, tableName>>

            // (1) build table dictionary
            for (var i = 0; i < source.Length; ++i)
            {
                if (source[i].GetEntityType() == EntityType.Table)
                {
                    if (!entityDico.ContainsKey(source[i].Id)) entityDico.Add(source[i].Id, new HashSet<string>());
                    if (!tableDico.ContainsKey(source[i].Id)) tableDico.Add(source[i].Id, source[i].Name);
                }
            }
            for (var i = 0; i < source.Length; ++i)
            {
                var meta = source[i];
                var relType = source[i].GetEntityType() == EntityType.Relation
                    ? source[i].GetRelationType()
                    : RelationType.NotDefined;
                if (meta.GetEntityType() == EntityType.Field || source[i].GetEntityType() == EntityType.Relation && (relType == RelationType.Mto || relType == RelationType.Otop))
                    if (entityDico.ContainsKey(source[i].RefId) && !entityDico[source[i].RefId].Contains(source[i].Name.ToUpper()))
                        entityDico[source[i].RefId].Add(source[i].Name.ToUpper());
            }
            // (2) check index definition
            for (var i = 0; i < source.Length; ++i)
            {
                if (source[i].GetEntityType() != EntityType.Index) continue;
                if (string.IsNullOrWhiteSpace(source[i].Value)) continue;  // avoid exception but definition is wrong
                if (!entityDico.ContainsKey(source[i].RefId)) continue;
                var meta = source[i];
                var fieldSet = entityDico[meta.RefId];
                var arrFieldList = source[i].Value.Split(Constants.IndexFieldSeparator);
                for (var j = 0; j < arrFieldList.Length; ++j)
                {
                    if (string.IsNullOrWhiteSpace(arrFieldList[j])) continue;
                    if (!fieldSet.Contains(arrFieldList[j].ToUpper()))
                        result.AddItem(Constants.MetadataInvalidIndexDef.Id, meta.LineNumber,
                            Constants.MetadataInvalidIndexDef.Name,
                            string.Format(Constants.MetadataInvalidIndexDef.Description, arrFieldList[j],
                                tableDico.TryGetValue(meta.RefId)),
                            Constants.MetadataDuplicateName.Level);
                }

            }

            return result;
        }


    }
}
