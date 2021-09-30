using Ring.Data;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Builders
{
    /// <summary>
    /// Map an objects to FieldExtension object (see GetInstance() overload)
    /// </summary>
    internal sealed class FieldBuilder : EntityBuilder
    {

        /// <summary>
        /// Add field object 
        /// </summary>
        public Field GetInstance(int id, string name, string description, FieldType fieldType, int size, bool notNull, bool caseSensitif = true) => new Field(
            id,
            name,
            description,
            fieldType,
            fieldType == FieldType.String && size <= 0 ? int.MaxValue : size,
            null,
            false,
            notNull,
            caseSensitif,
            true,
            false);

        /// <summary>
        /// Add field object from strings parameters
        /// </summary>
        public static Field GetInstance(int id, string name, FieldType fieldType, string description) =>
            new Field(id, name, description, FieldType.Long, 0, null, false, true, true, true, false);

        /// <summary>
        /// Add primary key field instance before constants initialization
        /// </summary>
        public Field GetInstance(DatabaseProvider provider, MetaData meta, SchemaLoadType loadType, SchemaSourceType sourceType)
        {
            var id = int.Parse(meta.Id);
            var size = meta.GetFieldSize();
            var type = meta.GetFieldType();
            var defaultValue = type.GetDefault(provider, meta.Value);
	        var isPrimaryKey = IsPrimaryKey(sourceType, meta.Name);

			if (type == FieldType.String && size <= 0) size = int.MaxValue;

            // add +1 to id to insert the primary key
            if (sourceType == SchemaSourceType.NativeXml && !isPrimaryKey) ++id;

			// is primary key definition
	        if (isPrimaryKey)
	        {
		        switch (sourceType)
		        {
			        case SchemaSourceType.ClfyDataBase: return Constants.DefaultClflyPrimaryKey;
			        case SchemaSourceType.NativeDataBase:
				        switch (type)
				        {
					        case FieldType.Int: return Constants.DefaultIntPrimaryKey;
					        case FieldType.Short: return Constants.DefaultShortPrimaryKey;
					        case FieldType.Long: return Constants.DefaultPrimaryKey;
				        }
				        break;
		        }
	        }
	        return new Field(
                id,
                meta.GetEntityName(),
                meta.GetEntityDescription(loadType),
                type,
                size,
                defaultValue,
                meta.IsBaselined(),
                meta.IsFieldNotNull(),
                meta.IsFieldCaseSensitif(),
                meta.IsEnabled(),
                meta.IsFieldMultilingual());
        }

        /// <summary>
        /// Generate field name
        /// </summary>
        public string GetFieldName(DatabaseProvider provider, TableType tableType, FieldKey fieldDescription)
        {
            switch (provider)
            {
                case DatabaseProvider.PostgreSql:
                    if (tableType == TableType.TableDictionary && fieldDescription == FieldKey.Name) return Constants.PostgreDictionaryName;
                    if (tableType == TableType.TableDictionary && fieldDescription == FieldKey.SchemaName) return Constants.PostgreDictionarySchemaName;
                    if (tableType == TableType.SchemaDictionary && fieldDescription == FieldKey.SchemaName) return Constants.PostgreDictionarySchemaNameField;
                    if (tableType == TableType.TableSpaceDictionary && fieldDescription == FieldKey.Name) return Constants.PostgreDictionaryTableSpaceName;
                    break;
            }
            return null;
        }

        /// <summary>
        /// Get sourceType id from an attribute name eg. 'Long', 'Int', 'String', ...
        /// </summary>
        /// <param name="typeName">Attribute name</param>
        /// <returns>Source Id</returns>
        public static int GetTypeId(string typeName) => typeName != null &&
             Constants.FieldTypeEnumsName?.ContainsKey(typeName.ToLower().Trim()) == true
            ? Constants.FieldTypeEnumsName[typeName.ToLower().Trim()] : Constants.NotDefinedFieldTypeId;

		
        /// <summary>
        /// Get default primary key field object
        /// </summary>
        /// <returns>field object</returns>
        public Field GetDefaultPrimaryKey(SchemaSourceType sourceType) =>
            sourceType == SchemaSourceType.ClfyXml || sourceType == SchemaSourceType.ClfyDataBase
                        ? Constants.DefaultClflyPrimaryKey : Constants.DefaultPrimaryKey;

	    #region private methods

		private static bool IsPrimaryKey(SchemaSourceType sourceType, string fieldName)
	    {
			if (sourceType == SchemaSourceType.ClfyDataBase && string.Equals(fieldName, Constants.DefaultClflyPrimaryKey.Name)) return true;
		    if ((sourceType == SchemaSourceType.NativeDataBase || sourceType == SchemaSourceType.NativeXml) &&
		        string.Equals(fieldName, Constants.DefaultPrimaryKey.Name)) return true;
		    return false;
	    }

		#endregion 
	}
}
