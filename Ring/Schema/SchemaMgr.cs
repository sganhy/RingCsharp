using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Enums;
using Ring.Schema.Builders;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Util;
using System.IO;
using System.Text;
using System.Xml;

namespace Ring.Schema
{
    /// <summary>
    /// Schema manager 
    /// </summary>
    public sealed class SchemaMgr
    {

        private readonly SchemaBuilder _schemaBuilder = new SchemaBuilder();

        /// <summary>
        /// Import a schema 
        /// </summary>
        public long Upgrade(Stream doc, DatabaseProvider provider, out string feedback)
        {
            feedback = string.Empty;
            var jobId = Global.Databases.GenerateNewJobId(JobType.Upgrade);

            // 1) already upgrading
            if (Global.Databases.Status != DatabaseCollectionStatus.Ready) return -1L;
            Global.Databases.SetStatus(DatabaseCollectionStatus.Upgrading);

            // 2) xml to Schema Object
            var newSchema = _schemaBuilder.GetInstance(doc, provider, SchemaLoadType.Full);

            // 3) validation ok ? 
            if (!newSchema.Feedback.IsBlockingDefect)
            {
                // 4) upgrade or creation 
                if (newSchema.Exists()) newSchema.Alter();
                else newSchema.Create();
            }

            Global.Databases.SetStatus(DatabaseCollectionStatus.Ready);
            return jobId;
        }

        /// <summary>
        /// Export schema 
        /// </summary>
        public static void Export(string file, int schemaId)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = Constants.IndentationChar.ToString(),
                Encoding = Encoding.UTF8
            };

            //TODO: define encoding 
            using (var xtw = XmlWriter.Create(file, settings))
            {
                var schema = Global.Databases.GetSchema(schemaId);
                if (schema == null) return;
                xtw.WriteStartDocument(true);
                xtw.WriteStartElement(schema.GetType().Name.ToLower()); // write <schema>

                // take sort by id
                for (var i = 0; i < schema.TablesById.Length; ++i)
                {
                    var table = schema.TablesById[i];
                    xtw.WriteStartElement(table.GetType().Name.ToLower()); // write <table>
                    xtw.WriteAttributeString(Constants.EntityName.ToLower(), table.Name); // table.name
                    xtw.WriteAttributeString(Constants.EntityId.ToLower(), table.Id.ToString()); // table.id

                    #region table description 
                    // description
                    if (!string.IsNullOrWhiteSpace(table.Description))
                    {
                        xtw.WriteStartElement(Constants.EntityDecription.ToLower()); // write <description>
                        xtw.WriteCData(table.Description);
                        xtw.WriteEndElement(); // write </description>
                    }
                    #endregion
                    #region fields
                    xtw.WriteStartElement(Constants.XmlFieldListName.ToLower()); // write <field_list>
                    for (var j = 0; j < table.FieldsById.Length; ++j)
                    {
                        var field = table.FieldsById[j];
                        if (field.Name == table.PrimaryKey.Name) continue; // skip primary key 
                        xtw.WriteStartElement(Constants.XmlFieldName.ToLower()); // write <field>
                        xtw.WriteAttributeString(Constants.EntityName.ToLower(), field.Name);
                        xtw.WriteAttributeString(Constants.FieldNameDefaultDataType,
                            NamingConvention.ToCamelCase(field.Type.ToString()));
                        if (field.Type == FieldType.String)
                        {
                            if (field.Size < int.MaxValue)
                                xtw.WriteAttributeString(Constants.FieldNameDefaultSize.ToLower(), field.Size.ToString());
                            if (!field.CaseSensitive)
                                xtw.WriteAttributeString(Constants.FieldCaseSensitif.ToLower(), bool.FalseString.ToLower());
                            if (field.NotNull)
                                xtw.WriteAttributeString(Constants.FieldNotNull.ToLower(), bool.TrueString.ToLower());
                        }
                        #region field description
                        if (!string.IsNullOrWhiteSpace(field.Description))
                        {
                            xtw.WriteStartElement(Constants.EntityDecription.ToLower()); // write <description>
                            xtw.WriteCData(field.Description);
                            xtw.WriteEndElement(); // write </description>
                        }
                        #endregion 
                        xtw.WriteEndElement(); // write </field>
                    }
                    xtw.WriteEndElement(); // write </field_list>
                    #endregion
                    #region relations
                    if (table.Relations.Length > 0)
                    {
                        xtw.WriteStartElement(Constants.XmlRelationListName.ToLower()); // write <relation_list>
                        for (var j = 0; j < table.Relations.Length; ++j)
                        {
                            var relation = table.Relations[j];
                            xtw.WriteStartElement(Constants.XmlRelationName.ToLower()); // write <relation>
                            xtw.WriteAttributeString(Constants.EntityName.ToLower(), relation.Name);
                            xtw.WriteAttributeString(Constants.XmlRelationTo.ToLower(),
                                relation.To != null ? relation.To.Name : string.Empty);
                            xtw.WriteAttributeString(Constants.XmlRelationType.ToLower(),
                                relation.Type.ToString().ToUpper());
                            xtw.WriteAttributeString(Constants.XmlRelationInverseRelation.ToLower(),
                                relation.InverseRelationName);
                            //xtw.WriteAttributeString(Constants.FieldNameDefaultDataType, field.Type.T
                            #region relation description
                            if (!string.IsNullOrWhiteSpace(relation.Description))
                            {
                                xtw.WriteStartElement(Constants.EntityDecription.ToLower()); // write <description>
                                xtw.WriteCData(relation.Description);
                                xtw.WriteEndElement(); // write </description>
                            }
                            #endregion
                            xtw.WriteEndElement(); // write </relation>
                        }
                        xtw.WriteEndElement(); // write </relation_list>
                    }
                    #endregion
                    #region indexes
                    if (table.Indexes.Length > 0)
                    {
                        xtw.WriteStartElement(Constants.XmlIndexListName.ToLower()); // write <index_list>
                        for (var j = 0; j < table.Indexes.Length; ++j)
                        {
                            var index = table.Indexes[j];
                            xtw.WriteStartElement(Constants.XmlIndexName.ToLower()); // write <index>
                            xtw.WriteAttributeString(Constants.EntityName.ToLower(), index.Name);
                            if (index.Unique) xtw.WriteAttributeString(Constants.XmlIndexUnique.ToLower(), bool.TrueString.ToLower());
                            #region index description
                            if (!string.IsNullOrWhiteSpace(index.Description))
                            {
                                xtw.WriteStartElement(Constants.EntityDecription.ToLower()); // write <description>
                                xtw.WriteCData(index.Description);
                                xtw.WriteEndElement(); // write </description>
                            }
                            #endregion
                            #region fields
                            for (var k = 0; k < index.Fields.Length; ++k)
                            {
                                var indexedField = index.Fields[k];
                                // is field or relation ? 
                                xtw.WriteStartElement(Constants.XmlIndexField.ToLower()); // write <field_index>
                                xtw.WriteString(indexedField.Name);
                                xtw.WriteEndElement(); // write </field_index>
                            }
                            #endregion 
                            xtw.WriteEndElement(); // write </index>
                        }
                        xtw.WriteEndElement(); // write </index_list>
                    }
                    #endregion

                    xtw.WriteEndElement(); // write </table>
                }
                xtw.WriteEndElement(); // write </schema>
                xtw.WriteEndDocument();
            }
        }

    }
}
