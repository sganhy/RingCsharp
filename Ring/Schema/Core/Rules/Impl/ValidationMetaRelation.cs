using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Collections.Generic;

namespace Ring.Schema.Core.Rules.Impl
{
    /// <summary>
    /// validation about relation definitions
    /// </summary>
    internal sealed class ValidationMetaRelation : IValidationRule<MetaData>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public ValidationResult Validate(MetaData[] source)
        {
            var result = new ValidationResult();
            result.Merge(CheckRelation(source));
            return result;
        }

        /// <summary>
        /// Check Relation inverse relation + inverse Type
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ValidationResult CheckRelation(MetaData[] source)
        {
            var result = new ValidationResult();
            // <relation.RefId, <relation.Name, RelationType>>
            var dicoRelation = new SortedDictionary<int, SortedDictionary<string, RelationType>>();
            var listOfRelation = new List<MetaData>();

            // (1) generate dictionary
            for (var i = 0; i < source.Length; ++i)
            {
                var meta = source[i];
                if (meta.GetEntityType() == EntityType.Relation)
                {
                    var relationName = meta.Name.ToUpper();
                    int objectId;
                    if (!int.TryParse(meta.RefId, out objectId)) continue;
                    if (!dicoRelation.ContainsKey(objectId)) dicoRelation.Add(objectId, new SortedDictionary<string, RelationType>());
                    if (!dicoRelation[objectId].ContainsKey(relationName))
                        dicoRelation[objectId].Add(relationName, meta.GetRelationType());
                    listOfRelation.Add(meta);
                }
            }

            // (2) check relations
            for (var i = 0; i < listOfRelation.Count; ++i)
            {
                var meta = listOfRelation[i];

                // target table exist ? + TODO add execption
                if (!dicoRelation.ContainsKey(meta.DataType)) continue;

                // check inverse relation    
                if (meta.Value == null || !dicoRelation[meta.DataType].ContainsKey(meta.Value.ToUpper()))
                {
                    result.AddItem(Constants.MetadataRelationInverse.Id, meta.LineNumber,
                        string.Format(Constants.MetadataRelationInverse.Name, meta.Name),
                        string.Format(Constants.MetadataRelationInverse.Description, meta.Value),
                        Constants.MetadataNameIsTooLong.Level);
                    continue;
                }

                // check opposit type
                var relationType = GetOppositRelationType(meta.GetRelationType());
                if (dicoRelation[meta.DataType][meta.Value.ToUpper()] != relationType)
                {
                    result.AddItem(Constants.MetadataRelationTypeInverse.Id, meta.LineNumber,
                        string.Format(Constants.MetadataRelationTypeInverse.Name, meta.Name),
                        string.Format(Constants.MetadataRelationTypeInverse.Description, meta.Value),
                        Constants.MetadataNameIsTooLong.Level);
                }
            }

            return result;
        }


        /// <summary>
		/// Get opposit Relation Type
		/// </summary>
		/// <param name="relationType"></param>
		/// <returns></returns>
	    private static RelationType GetOppositRelationType(RelationType relationType)
        {
            switch (relationType)
            {
                case RelationType.Mtm: return RelationType.Mtm;
                case RelationType.Otof: return RelationType.Otop;
                case RelationType.Otop: return RelationType.Otof;
                case RelationType.Otm: return RelationType.Mto;
                case RelationType.Mto: return RelationType.Otm;
            }
            return RelationType.NotDefined;

        }

    }
}
