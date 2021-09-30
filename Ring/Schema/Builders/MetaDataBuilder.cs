using Ring.Data.Core;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Mappers;
using Ring.Schema.Models;
using Ring.Util.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Schema.Builders
{
    internal sealed class MetaDataBuilder : EntityBuilder
    {
        private const EntityType DefaultEntityType = EntityType.NotDefined;

        /// <summary>
        /// Get instances from table 
        /// </summary>
        public MetaData[] GetInstances(Table table)
        {
            var tableId = table.Id.ToString();
            var result = new List<MetaData>
            {
                new MetaData
                {
                    Id = table.Id.ToString(),
                    Name = table.Name,
                    RefId = Constants.DefaultRefId,
                    ObjectType = (sbyte) EntityType.Table,
                    DataType = 0,
                    Flags = GetFlags(table),
                    LineNumber = 0L,
                    Value = table.Subject,
                    Description = table.Description
                }
            };
            for (var i = 0; i < table.Fields.Length; ++i)
                result.Add(GetInstance(tableId, table.Fields[i]));
            for (var i = 0; i < table.Relations.Length; ++i)
                result.Add(GetInstance(tableId, table.Relations[i]));
            for (var i = 0; i < table.Indexes.Length; ++i)
                result.Add(GetInstance(table.Indexes[i]));
            return result.ToArray();
        }

        /// <summary>
        /// Calculate flags for a table 
        /// </summary>
        public long GetFlags(Table table)
        {
            var meta = new MetaData { Flags = Constants.DefaultFlags };
            meta.SetBaseline(table.Baseline);
            meta.SetEnabled(table.Active);
            meta.SetReadonly(table.Readonly);
            meta.SetTableCacheId(table.CacheId);
            return meta.Flags;
        }

        /// <summary>
        /// Get unique metada from schema 
        /// </summary>
        public MetaData GetInstance(Database schema)
        {
            var result = new MetaData
            {
                //schema.Id.ToString(),
                Id = schema.Id.ToString(),
                Name = schema.Name,
                RefId = Constants.DefaultRefId,
                ObjectType = (sbyte)EntityType.Schema,
                DataType = schema.DefaultPort,
                Flags = Constants.DefaultFlags,
                LineNumber = 0L,
                Value = schema.SearchPath,
                Description = schema.ConnectionString
            };

            switch (schema.Source)
            {
                case SchemaSourceType.ClfyXml: result.SetSchemaAsClfy(); break;
                case SchemaSourceType.NativeXml: result.SetSchemaAsNative(); break;
                case SchemaSourceType.NativeDataBase:
                case SchemaSourceType.ClfyDataBase:
                case SchemaSourceType.NotDefined:
                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }

        /// <summary>
        /// Get meta from language
        /// </summary>
        public MetaData GetInstance(Language language)
        {
            var result = new MetaData
            {
                //schema.Id.ToString(),
                Id = language.Id.ToString(),
                Name = language.Name,
                RefId = 0.ToString(),
                ObjectType = (sbyte)EntityType.Language,
                DataType = 0,
                Flags = Constants.DefaultFlags,
                LineNumber = 1,
                Value = null,
                Description = language.Description
            };
            return result;
        }

        /// <summary>
        /// Generate Metada from a Xml/Json file
        /// </summary>
        public MetaData[] GetInstances(Stream document)
        {
            var metalist = new List<MetaData>();
            if (document == null) return metalist.ToArray();
            var detectClfySchema = IsClarifySChema(document);
            var dicoTable = new Dictionary<string, string>();

            #region  load delegates

            MetaDataFactory factory = DefaultMetaFactory; // delegate to build MetaData object

            if (detectClfySchema)
            {
                factory = ClfytMetaFactory;
            }

            #endregion
            #region build dico <lower(table_name),table_id>

            document.Position = 0L; // start from begining - very important !! 
            var readerSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                CheckCharacters = false,
                IgnoreComments = true
            };
            using (var xmlTextReader = XmlReader.Create(document, readerSettings))
            {
                // build dico <lower(table_name), table_id>
                // first pass - read all tables 
                while (xmlTextReader.Read())
                {
                    if (!xmlTextReader.IsStartElement()) continue;
                    if (!xmlTextReader.HasAttributes) continue;
                    if (xmlTextReader.NodeType != XmlNodeType.Element) continue;
                    if (!string.Equals(xmlTextReader.Name, Constants.EntityTableName, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(xmlTextReader.Name, Constants.EntityObjectName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var tableName = XmlHelper.GetAttributeValue(xmlTextReader, Constants.EntityName);
                    if (tableName == null) continue;
                    if (!dicoTable.ContainsKey(tableName.Trim().ToLower()))
                        dicoTable.Add(tableName.Trim().ToLower(),
                            GetTableId(xmlTextReader, detectClfySchema));
                }
            }

            #endregion
            #region mapping methods
            document.Position = 0L; // start from begining - very important !! 
            var tableSpaceId = 0L;

            using (var xmlTextReader = XmlReader.Create(document))
            {
                while (xmlTextReader.Read())
                {
                    if (!xmlTextReader.IsStartElement()) continue;
                    if (!xmlTextReader.HasAttributes) continue;
                    if (xmlTextReader.NodeType != XmlNodeType.Element) continue;

                    // is schema ? first read
                    IXmlLineInfo readerInfo;
                    if (xmlTextReader.Name.IndexOf(Constants.EntitySchemaName, StringComparison.OrdinalIgnoreCase) >= 0 && metalist.Count == 0)
                    {
                        readerInfo = (IXmlLineInfo)xmlTextReader;
                        // get schema info
                        metalist.Add(factory(xmlTextReader, Constants.DefaultIdValue, Constants.DefaultRefId, 0, EntityType.Schema, readerInfo.LineNumber));
                        // get language info 
                        if (!detectClfySchema)
                            metalist.Add(
                                factory(xmlTextReader, Constants.DefaultIdValue,
                                Constants.DefaultRefId, 0, EntityType.Language, readerInfo.LineNumber));
                    }

                    if (string.Equals(xmlTextReader.Name, Constants.EntityTableSpaceName, StringComparison.OrdinalIgnoreCase))
                    {
                        readerInfo = (IXmlLineInfo)xmlTextReader;
                        metalist.Add(factory(xmlTextReader, Constants.DefaultIdValue, tableSpaceId++.ToString(), 0, EntityType.TableSpace, readerInfo.LineNumber));
                    }

                    // is table ?
                    if (!string.Equals(xmlTextReader.Name, Constants.EntityObjectName, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(xmlTextReader.Name, Constants.EntityTableName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    readerInfo = (IXmlLineInfo)xmlTextReader;  // warning linenumber stored on Int32

                    // read child node fields/relations/indexes 
                    using (var childList = xmlTextReader.ReadSubtree())
                    {
                        // MetaDataFactory(XmlReader node, long refId, string id, long dataType, EntityType entityType, long lineNumber)
                        var currentMeta = factory(xmlTextReader, Constants.DefaultIdValue, Constants.DefaultRefId, 0, EntityType.Table, readerInfo.LineNumber);
                        var relationId = 0L;
                        var fieldId = 0L;
                        var indexId = 0L;

                        metalist.Add(currentMeta); // Add table

                        do
                        {
                            if (childList.NodeType != XmlNodeType.Element) continue;
                            switch (GetEntityType(childList, detectClfySchema))
                            {
                                case EntityType.Field:
                                    readerInfo = (IXmlLineInfo)childList;  // warning linenumber stored on Int32
                                    metalist.Add(factory(childList, currentMeta.Id, fieldId++.ToString(), 0, EntityType.Field, readerInfo.LineNumber));
                                    break;
                                case EntityType.Relation:
                                    readerInfo = (IXmlLineInfo)childList;  // warning linenumber stored on Int32
                                    metalist.Add(factory(childList, currentMeta.Id, relationId++.ToString(),
                                        GetTableId(childList, dicoTable, detectClfySchema),
                                        EntityType.Relation, readerInfo.LineNumber));
                                    break;
                                case EntityType.Index:
                                    readerInfo = (IXmlLineInfo)childList;  // warning linenumber stored on Int32
                                    metalist.Add(factory(childList, currentMeta.Id, indexId++.ToString(), 0, EntityType.Index, readerInfo.LineNumber));
                                    break;
                                case EntityType.Table:
                                case EntityType.Schema:
                                    break;
                                case EntityType.Comment:
                                    SetDescription(metalist, childList.ReadString());
                                    break;
                                case EntityType.NotDefined:
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                        } while (childList.Read());
                    }
                }
            }
            #endregion

            return metalist.ToArray();
        }

        /// <summary>
        /// Generate Metadata from @meta table result
        /// </summary>
        public MetaData[] GetInstances(Data.List list)
        {
            if (list == null) return new MetaData[0];
            var metalist = new List<MetaData>(list.Count);
            for (var i = 0; i < list.Count; ++i) metalist.Add(MetaDataMapper.Map(list[i]));
            return metalist.ToArray();
        }

        /// <summary>
        /// Generate Metadata from @meta table result
        /// </summary>
        public static MetaData GetNullInstance() => new MetaData { ObjectType = (sbyte)EntityType.Null, DataType = 0, Flags = 0L, LineNumber = 0L };

        /// <summary>
        /// Get Metadata instances from field object - TODO sequence on table 
        /// </summary>
        public MetaData GetInstance(Sequence sequence)
        {
            var result = new MetaData
            {
                Id = sequence.Id.ToString(),
                Name = sequence.Name,
                RefId = Constants.DefaultRefId,
                ObjectType = (sbyte)EntityType.Sequence,
                DataType = (int)FieldType.Long,
                Flags = Constants.DefaultFlags,
                LineNumber = 0L,
                Value = sequence.MaxValue.ToString(),
                Description = sequence.Description
            };
            if (sequence.Value.ReservedRange > 0) result.SetSequenceCacheId(true);
            return result;
        }

        /// <summary>
        /// Get Metadata instances from field object
        /// </summary>
        public MetaData GetInstance(string refId, Field field)
        {
            var result = new MetaData
            {
                Id = field.Id.ToString(),
                Name = field.Name,
                RefId = refId,
                ObjectType = (sbyte)EntityType.Field,
                DataType = field.Type.GetId(),
                Flags = Constants.DefaultFlags,
                LineNumber = 0L,
                Value = field.DefaultValue,
                Description = field.Description
            };
            result.SetBaseline(field.Baseline);
            result.SetEnabled(true);
            result.SetFieldSize(field.Size);
            result.SetFieldNotNull(field.NotNull);
            result.SetFieldCaseSensitif(field.CaseSensitive);
            result.SetFieldMultilingual(field.Multilingual);
            return result;
        }

        /// <summary>
        /// Get Metadata instances from Index object
        /// </summary>
        public MetaData GetInstance(Index index)
        {
            var value = new StringBuilder();

            for (var i = 0; i < index.Fields.Length; ++i)
            {
                value.Append(index.Fields[i].Name);
                if (i < index.Fields.Length - 1) value.Append(Constants.IndexFieldSeparator);
            }
            var result = new MetaData
            {
                Id = index.Id.ToString(),
                Name = index.Name,
                RefId = index.TableId.ToString(),
                ObjectType = (sbyte)EntityType.Index,
                DataType = 0,
                Flags = Constants.DefaultFlags,
                LineNumber = 0L,
                Value = value.ToString(),
                Description = index.Description
            };
            result.SetBaseline(index.Baseline);
            result.SetEnabled(true);
            result.SetIndexUnique(index.Unique);
            return result;
        }

        /// <summary>
        /// Get Metadata instances from TableSpace object
        /// </summary>
        public MetaData GetInstance(TableSpace tableSpace)
        {
            var result = new MetaData
            {
                Id = tableSpace.Id.ToString(),
                Name = tableSpace.Name,
                RefId = Constants.DefaultRefId, // schemaId
                ObjectType = (sbyte)EntityType.TableSpace,
                DataType = 0,
                Flags = Constants.DefaultFlags,
                LineNumber = 0L,
                Value = tableSpace.FileName,
                Description = tableSpace.Description
            };
            result.SetBaseline(tableSpace.Baseline);
            result.SetEnabled(true);
            result.SetTableSpaceReadonly(tableSpace.IsReadonly);
            result.SetTablespaceTable(tableSpace.IsTable);
            result.SetTablespaceIndex(tableSpace.IsIndex);
            return result;
        }


        /// <summary>
        /// Get Metadata instances from relation object
        /// </summary>
        public MetaData GetInstance(string refId, Relation relation)
        {
            var dataType = relation?.To.Id ?? 0;
            var result = new MetaData
            {
                Id = relation?.Id.ToString(),
                Name = relation?.Name,
                RefId = refId,
                ObjectType = (sbyte)EntityType.Relation,
                DataType = dataType,
                Flags = Constants.DefaultFlags,
                LineNumber = 0L,
                Value = relation.GetInverseRelation()?.Name,
                Description = relation?.Description
            };
            result.SetBaseline(relation?.Baseline ?? false);
            result.SetEnabled(relation?.Active ?? false);
	        result.SetRelationdNotNull(relation?.NotNull ?? false);
	        result.SetRelationConstraint(relation?.Constraint ?? false);
			result.SetRelationType(relation?.Type ?? RelationType.NotDefined);
            return result;
        }

        #region private methods

        private delegate MetaData MetaDataFactory(XmlReader node, string refId, string id, int dataType, EntityType entityType, long lineNumber);

        /// <summary>
        /// Converter clfy data Source to FieldType
        /// </summary>
        /// <param name="value">DataTypeId</param>
        /// <param name="description">DataTypeDesc</param>
        /// <returns></returns>
        private static FieldType GetClfyDataType(int value, string description)
        {
            switch (value)
            {
                case 0:
                    return
                    Constants.FieldDataTypeArray.Equals(description, StringComparison.OrdinalIgnoreCase) ?
                    FieldType.Array : FieldType.Long;
                case 1: return FieldType.Short;
                case 2: return FieldType.Byte;
                case 3: return FieldType.Double;
                case 7: return FieldType.String;
                case 8: return FieldType.String;
                case 9: return FieldType.DateTime;
                case 11: return FieldType.Double;
            }
            return FieldType.NotDefined;
        }

        /// <summary>
        /// Converter clfy data Source to FieldType
        /// </summary>
        /// <param name="value">DataTypeId</param>
        /// <param name="description">DataTypeIdDesc</param>
        /// <returns></returns>
        private static FieldType GetClfyDataType(string value, string description)
        {
            int temp;
            if (!int.TryParse(value, out temp)) temp = int.MaxValue;
            return GetClfyDataType(temp, description);
        }

        /// <summary>
        /// Format fields list for indexes 
        /// </summary>
        private static string GetFields(XmlReader node, bool clfySchema)
        {
            var result = new StringBuilder();
            node.MoveToElement();
            using (var childList = node.ReadSubtree())
            {
                do
                {
                    // field_id(0); field_id(1)
                    if (string.Equals(XmlHelper.FormatAttribute(childList.Name), Constants.IndexFieldDefinition,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        if (clfySchema) result.Append(XmlHelper.GetAttributeValue(node, Constants.EntityName) + Constants.IndexFieldSeparator);
                        else if (childList.NodeType == XmlNodeType.Element)
                            result.Append(XmlHelper.GetAttributeValue(node, Constants.EntityName).Trim() + Constants.IndexFieldSeparator);
                    }
                }
                while (childList.Read());
            }
            return result.ToString();
        }

        /// <summary>
        /// Default entity factory 
        /// </summary>
        private static MetaData DefaultMetaFactory(XmlReader node, string refId, string id, int dataType, EntityType entityType, long lineNumber)
        {
            // MetaDataFactory(XmlReader node, string refId, string id, long dataType, EntityType entityType, long lineNumber)
            if (!node.HasAttributes) return Constants.NullMetada;

            var result = new MetaData
            {
                Id = id,
                Flags = Constants.DefaultFlags,
                RefId = refId,
                Name = XmlHelper.GetAttributeValue(node, Constants.EntityName),
                Description = string.Empty,
                ObjectType = entityType.GetId(),
                DataType = 0,
                LineNumber = lineNumber
            };
            result.Value = result.Name;
            result.SetEnabled(true);

            var attributeValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameBaseLine);
            if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase))
                result.SetBaseline(true);
            else result.SetBaseline(false);

            switch (entityType)
            {
                case EntityType.TableSpace:
                    #region manage tablespace
                    result.Value = XmlHelper.GetAttributeValue(node, Constants.TableSpaceFile);
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.EntityTableName);
                    if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase)) result.SetTablespaceTable(true);
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.EntityIndexName);
                    if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase)) result.SetTablespaceIndex(true);
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.TableNameReadonly);
                    if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase)) result.SetTableSpaceReadonly(true);
                    #endregion
                    break;
                case EntityType.Table:
                    #region manage table
                    result.Id = GetTableId(node, false);
                    result.RefId = result.Id; // to improve sort algo
                    // by default cache Id - TODO manage attribute cacheId
                    result.SetTableCacheId(true);

                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.TableNameReadonly);
                    if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase))
                        result.SetReadonly(true);
                    else result.SetReadonly(false);

                    #endregion
                    break;
                case EntityType.Field:
                    #region manage field 
                    long size;
                    result.Value = null;

                    // get field size 
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameDefaultSize);
                    if (attributeValue != null && long.TryParse(attributeValue.Trim(), out size)) result.SetFieldSize(size);

                    // is multilingual ?
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.FieldMultilingual);
                    if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase)) result.SetFieldMultilingual(true);
                    else result.SetFieldMultilingual(false);

                    // is not null ?
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameDefaultNotNull);
                    if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase)) result.SetFieldNotNull(true);
                    else result.SetFieldNotNull(false);

                    // is case sensitif ?
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameDefaultCaseSensitif);
                    if (attributeValue != null && string.Equals(bool.FalseString, attributeValue, StringComparison.OrdinalIgnoreCase)) result.SetFieldCaseSensitif(false);
                    else result.SetFieldCaseSensitif(true);

                    // get default value 
                    result.Value = XmlHelper.GetAttributeValue(node, Constants.FieldDefaultValue);

                    // get dataType
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameDefaultDataType);
                    if (!string.IsNullOrEmpty(attributeValue))
                        result.DataType = FieldBuilder.GetTypeId(attributeValue);

                    // copy datatype definition to meta.value (used by validations)
                    if (result.DataType == Constants.NotDefinedFieldTypeId)
                        result.Value = attributeValue;

                    #endregion
                    break;
                case EntityType.Relation:
                    #region manage relation
                    // relation type 
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.RelNameDefaultRelationType);
                    if (attributeValue != null) result.SetRelationType(attributeValue);
                    result.RefId = refId;
                    result.DataType = dataType;
                    result.Value = XmlHelper.GetAttributeValue(node, Constants.RelNameInverseRelation);
                    if (dataType == Constants.TableIdNotFound)
                        result.Value = XmlHelper.GetAttributeValue(node, Constants.RelNameToTable);

					attributeValue = XmlHelper.GetAttributeValue(node, Constants.RelNotNull); // same tag
					// not null relation
	                if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase)) result.SetRelationdNotNull(true);
	                else result.SetRelationdNotNull(false);

	                attributeValue = XmlHelper.GetAttributeValue(node, Constants.RelContraint); // same tag
					// add foreign key
					if (attributeValue != null && string.Equals(bool.FalseString, attributeValue, StringComparison.OrdinalIgnoreCase)) result.SetRelationConstraint(false); 
	                else result.SetRelationConstraint(true);

					#endregion
					break;
                case EntityType.Index:
                    #region manage index 
                    attributeValue = XmlHelper.GetAttributeValue(node, Constants.IndexUnique);
                    if (attributeValue != null && string.Equals(bool.TrueString, attributeValue, StringComparison.OrdinalIgnoreCase))
                        result.SetIndexUnique(true);
                    else result.SetIndexUnique(false);
                    result.Value = GetFields(node, false);
                    #endregion 
                    break;
                case EntityType.Schema:
                    #region manage schema
                    result.Flags = 0L;
                    result.SetSchemaAsNative();
                    if (result.Name == null) result.Name = Constants.DefaultSchemaName;
                    // **** calculate Id !!!! HERE !!!! ****
                    result.Id = Global.Databases.GetSchema(result.Name)?.Id.ToString() ?? Global.Databases.GetSchemaId().ToString();
                    result.RefId = Constants.DefaultRefId;
                    if (!int.TryParse(XmlHelper.GetAttributeValue(node, Constants.SchemaDefaultPort), out result.DataType))
                        result.DataType = Constants.DefaultSchemaPort;
                    result.Value = XmlHelper.GetAttributeValue(node, Constants.SchemaSearchPath);
                    // connection string 
                    result.Description = XmlHelper.GetAttributeValue(node, Constants.SchemaConnString);
                    #endregion
                    break;
                case EntityType.Language:
                    #region default language
                    result.Name = XmlHelper.GetAttributeValue(node, Constants.SchemaDefaultLanguage);
                    result.SetBaseline(true);
                    #endregion 
                    break;
                case EntityType.NotDefined:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Clarify entity factory 
        /// </summary>
        private static MetaData ClfytMetaFactory(XmlReader node, string refId, string id, int dataType, EntityType entityType, long lineNumber)
        {
            // MetaDataFactory(XmlReader node, string refId, string id, long dataType, EntityType entityType, long lineNumber)
            if (!node.HasAttributes) return Constants.NullMetada;

            var result = new MetaData
            {
                Id = id,
                Flags = Constants.DefaultFlags,
                RefId = refId,
                Name = XmlHelper.GetAttributeValue(node, Constants.EntityName),
                Description = string.Empty,
                ObjectType = entityType.GetId(),
                DataType = 0,
                LineNumber = lineNumber
            };
            result.Value = result.Name;
            result.SetEnabled(true);

            // is baseline ?
            var attrbValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameBaseLine);
            if (attrbValue != null && string.Equals(bool.TrueString, attrbValue, StringComparison.OrdinalIgnoreCase)) result.SetBaseline(true);
            else result.SetBaseline(false);

            switch (entityType)
            {
                case EntityType.Field:
                    #region manage field 
                    long size;
                    //TODO: redo Id for primary keys 
                    // get field size 
                    attrbValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameSize);
                    if (attrbValue != null && long.TryParse(attrbValue.Trim(), out size)) result.SetFieldSize(size);

                    // is not null ?
                    attrbValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameNotNull);
                    if (attrbValue != null && string.Equals(bool.FalseString, attrbValue, StringComparison.OrdinalIgnoreCase)) result.SetFieldNotNull(true);
                    else result.SetFieldNotNull(false);

                    // is case sensitif ?
                    attrbValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameCaseInsensitif);
                    if (attrbValue != null && string.Equals(bool.TrueString, attrbValue, StringComparison.OrdinalIgnoreCase))
                        result.SetFieldCaseSensitif(false);
                    else result.SetFieldCaseSensitif(true);

                    // get default value
                    result.Value = XmlHelper.GetAttributeValue(node, Constants.FieldDefaultValue);

                    // get dataType
                    attrbValue = XmlHelper.GetAttributeValue(node, Constants.FieldNameDataType);
                    if (!string.IsNullOrEmpty(attrbValue))
                        result.DataType = GetClfyDataType(attrbValue,
                            XmlHelper.GetAttributeValue(node, Constants.FieldNameDataTypeDesc)).GetId();

                    // copy datatype definition to meta.value
                    if (result.DataType == Constants.NotDefinedFieldTypeId)
                        result.Value = attrbValue;

                    #endregion 
                    break;
                case EntityType.Relation:
                    #region manage relation
                    // relation type 
                    attrbValue = XmlHelper.GetAttributeValue(node, Constants.RelNameRelationType);
                    if (attrbValue != null) result.SetRelationType(attrbValue);
                    result.RefId = refId;
                    result.DataType = dataType;
                    result.Value = XmlHelper.GetAttributeValue(node, Constants.RelNameInverseRelation);
                    if (dataType == Constants.TableIdNotFound)
                        result.Value = XmlHelper.GetAttributeValue(node, Constants.RelNameToTable);
                    #endregion
                    break;
                case EntityType.Index:
                    #region manage index
                    // is unique ?
                    attrbValue = XmlHelper.GetAttributeValue(node, Constants.IndexUnique);
                    if (attrbValue != null && string.Equals(bool.TrueString, attrbValue, StringComparison.OrdinalIgnoreCase))
                        result.SetIndexUnique(true);
                    else result.SetIndexUnique(false);
                    result.Value = GetFields(node, true);
                    #endregion 
                    break;
                case EntityType.Table:
                    #region manage table
                    result.Id = GetTableId(node, true);
                    result.Value = XmlHelper.GetAttributeValue(node, Constants.TableNameClfySubject);
                    result.RefId = result.Id; // to improve sort algo
                    #endregion 
                    break;
                case EntityType.Schema:
                    #region manage schema
                    result.Flags = 0L;
                    if (result.Name == null) result.Name = Constants.DefaultSchemaName;
                    result.SetSchemaAsClfy();
                    result.Id = Global.Databases.GetSchema(result.Name)?.Id.ToString() ?? Global.Databases.GetSchemaId().ToString();
                    result.Value = result.Name;
                    result.Description = XmlHelper.GetAttributeValue(node, Constants.SchemaConnString);
                    #endregion
                    break;
                case EntityType.NotDefined:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Get the table id from attribute value
        /// </summary>
        private static string GetTableId(XmlReader reader, bool clfySchema) =>
            clfySchema ? XmlHelper.GetAttributeValue(reader, Constants.FieldNameTypeId) :
                        XmlHelper.GetAttributeValue(reader, Constants.EntityId);

        /// <summary>
        /// Get target table for a relation
        /// </summary>
        private static int GetTableId(XmlReader node, Dictionary<string, string> tableCollection, bool clfySchema)
        {
            var attributeValue = clfySchema ?
                XmlHelper.GetAttributeValue(node, Constants.RelNameToTable) :
                XmlHelper.GetAttributeValue(node, Constants.RelationToObject);
            int result;

            // reset position after reading !!
            node.MoveToElement();

            if (attributeValue != null && tableCollection.ContainsKey(attributeValue.Trim().ToLower()) &&
                (tableCollection[attributeValue.Trim().ToLower()] != null &&
                 int.TryParse(tableCollection[attributeValue.Trim().ToLower()], out result))) return result;

            return Constants.TableIdNotFound;
        }

        /// <summary>
        /// Set description to the last meta element of metalist
        /// </summary>
        private static void SetDescription(List<MetaData> metalist, string description)
        {
            if (metalist == null) throw new ArgumentNullException(nameof(metalist));
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (metalist.Count > 0)
            {
                var metaData = metalist[metalist.Count - 1];
                metaData.Description = description;
                metalist[metalist.Count - 1] = metaData;
            }
        }

        /// <summary>
        /// Define l'entity type from Xml Document
        /// </summary>
        private static EntityType GetEntityType(XmlReader node, bool clfySchema)
        {
            if (node == null) return DefaultEntityType;
            if (clfySchema && string.Equals(node.Name, Constants.FieldDescription, StringComparison.OrdinalIgnoreCase)) return EntityType.Comment;
            if (!clfySchema && string.Equals(node.Name, Constants.EntityDecription, StringComparison.OrdinalIgnoreCase)) return EntityType.Comment;
            if (!node.HasAttributes) return DefaultEntityType;
            //important:  don't use typeof() due to obfuscation
            if (string.Equals(node.Name, Constants.EntityFieldName, StringComparison.OrdinalIgnoreCase)) return EntityType.Field;
            if (string.Equals(node.Name, Constants.EntityRelationName, StringComparison.OrdinalIgnoreCase)) return EntityType.Relation;
            if (string.Equals(node.Name, Constants.RelNameDefaultExclRelation, StringComparison.OrdinalIgnoreCase)) return EntityType.Relation;
            if (string.Equals(node.Name, Constants.EntityIndexName, StringComparison.OrdinalIgnoreCase)) return EntityType.Index;
            if (string.Equals(node.Name, Constants.EntitySchemaName, StringComparison.OrdinalIgnoreCase)) return EntityType.Schema;
            if (clfySchema && string.Equals(node.Name, Constants.EntityObjectName, StringComparison.OrdinalIgnoreCase)) return EntityType.Table;
            if (!clfySchema && string.Equals(node.Name, Constants.EntityTableName, StringComparison.OrdinalIgnoreCase)) return EntityType.Table;
            return DefaultEntityType;
        }

        #endregion

    }
}
