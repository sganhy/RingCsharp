using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Text;

namespace Ring.Schema.Builders
{
    /// <summary>
    /// Map an objects to Relation object (see GetInstance() overload)
    /// </summary>
    internal sealed class RelationBuilder : EntityBuilder
    {

        /// <summary>
        /// Add primary key field instance before constants initialization
        /// </summary>
        /// <returns></returns>
        public Relation GetInstance(MetaData meta, MetaData table, Table toTable, SchemaLoadType loadType, SchemaSourceType schemaSourceType,
            int inverseRelationId)
        {
            var type = meta.GetRelationType();
            return new Relation(int.Parse(meta.Id),
                                meta.GetEntityName(),
                                meta.GetEntityDescription(),
                                type,
                                toTable,
                                type == RelationType.Mtm ? GetNtmName(schemaSourceType, meta, table, inverseRelationId) : null,
                                meta.Value,
								type == RelationType.Mtm || meta.IsRelationNotNull(),
								meta.IsRelationConstraint(),
                                meta.IsBaselined(),
                                meta.IsEnabled());

        }


		#region private methods 

		/// <summary>
		/// Return the physical NtmName ( length is less than 30 for Oracle support)
		///          @mtm_ 10 _ 10 _ 3   = 5 +  10 + 1 + 10 + 1 + 3  = 30 (max)
		///			 @mtm_{min(table_id, table_id)}_{max(table_id, table_id)}_{relation_id}
		/// </summary>
		/// <param name="schemaSourceType">SchemaExtension Source</param>
		/// <param name="meta">meta current relation</param>
		/// <param name="table">meta table</param>
		/// <param name="inverseRelationId">inverse relation id</param>
		private static string GetNtmName(SchemaSourceType schemaSourceType, MetaData meta, MetaData table, int inverseRelationId)
        {
            var result = new StringBuilder();
            var fromTableId = int.Parse(table.Id);

            switch (schemaSourceType)
            {
                case SchemaSourceType.NativeXml:
                case SchemaSourceType.ClfyXml:
                case SchemaSourceType.ClfyDataBase:
                case SchemaSourceType.NativeDataBase:
	            {
		            result.Append(Constants.MtmPrefix);
		            // should be unique !! 
		            if (meta.DataType >= fromTableId)
		            {
			            result.Append(fromTableId.ToString().PadLeft(5, Constants.MtmPaddingChar));
			            result.Append(Constants.MtmSeperator);
			            result.Append(meta.DataType.ToString().PadLeft(5, Constants.MtmPaddingChar));
			            result.Append(Constants.MtmSeperator);

			            // source relation Id 
			            if (meta.DataType == fromTableId)
				            result.Append(System.Math.Min(int.Parse(meta.Id),
					            inverseRelationId).ToString().PadLeft(3, Constants.MtmPaddingChar));
			            else result.Append(meta.Id.PadLeft(3, Constants.MtmPaddingChar));
		            }
		            else
		            {
			            result.Append(meta.DataType.ToString().PadLeft(5, Constants.MtmPaddingChar));
			            result.Append(Constants.MtmSeperator);
			            result.Append(fromTableId.ToString().PadLeft(5, Constants.MtmPaddingChar));
			            result.Append(Constants.MtmSeperator);
			            // source relation Id 
			            result.Append(inverseRelationId.ToString().PadLeft(3, Constants.MtmPaddingChar));
		            }
		            break;
	            }
            }
            return result.ToString();
        }

        #endregion

    }
}
