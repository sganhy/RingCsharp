using Ring.Schema.Models;

namespace Ring.Schema
{
    internal static class Constants
    {
        /// <summary>
        /// SchemaMgr class constants
        /// </summary>
		internal static readonly string EntityName = Builders.Constants.EntityName;
        internal static readonly string EntityDecription = Builders.Constants.EntityDecription;
        internal static readonly string EntityId = Builders.Constants.EntityId;

        // field 
        internal static readonly string FieldNameDefaultDataType = Builders.Constants.FieldNameDefaultDataType; // copy 
        internal static readonly string FieldNameDefaultSize = Builders.Constants.FieldNameDefaultSize; // copy 
        internal static readonly string FieldCaseSensitif = Builders.Constants.FieldCaseSensitif;
        internal static readonly string FieldNotNull = Builders.Constants.FieldNotNull;

        // xml tags 
        internal const char IndentationChar = '\t';
        internal static readonly string XmlFieldName = nameof(Field);
        internal static readonly string XmlFieldListName = XmlFieldName + "_list";
        internal static readonly string XmlRelationName = nameof(Relation);
        internal static readonly string XmlRelationListName = XmlRelationName + "_list";
        internal static readonly string XmlIndexName = nameof(Index);
        internal static readonly string XmlIndexListName = XmlIndexName + "_list";
        internal static readonly string XmlRelationTo = "To";
        internal static readonly string XmlRelationType = Builders.Constants.RelNameDefaultRelationType;
        internal static readonly string XmlRelationInverseRelation = Builders.Constants.RelNameDefaultInverseRelation;
        internal static readonly string XmlIndexUnique = Builders.Constants.IndexUnique;
        internal static readonly string XmlIndexField = Builders.Constants.IndexFieldDefault;

    }

}
