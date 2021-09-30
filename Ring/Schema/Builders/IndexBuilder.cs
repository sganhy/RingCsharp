using System.Collections.Generic;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders
{
    internal sealed class IndexBuilder : EntityBuilder
    {
        public Index GetInstance(MetaData meta, BaseEntity[] fields, Table table) =>
            new Index(
                int.Parse(meta.Id),
                meta.GetEntityName(),
                meta.GetEntityDescription(),
                fields,
                table.Id,
                meta.IsIndexUnique(),
                meta.IsIndexBitmap(),
                meta.IsEnabled(),
                meta.IsBaselined());

        public Index GetInstance(int id, string name, string description, Table table, string fieldName, bool unique, bool bitmap) =>
            new Index(
                id,
                name,
                description,
                new BaseEntity[] { table.GetField(fieldName) },
                table.Id,
                unique,
                bitmap,
                true,
                true);

        public Index GetInstance(int id, string name, string description, Table table, string[] fieldName,
            bool unique, bool bitmap)
        {
            if (fieldName == null || fieldName.Length == 0) return null;
            var result = new Index(id,
                name,
                description,
                new BaseEntity[fieldName.Length],
                table.Id,
                unique,
                bitmap,
                true,
                true);
            for (var i = 0; i < fieldName.Length; ++i) result.Fields[i] = table.GetField(fieldName[i]);
            return result;
        }

        public Index GetInstance(int id, Index index) =>
            new Index(
                id,
                index.Name,
                index.Description,
                index.Fields,
                index.TableId,
                index.Unique,
                index.Bitmap,
                index.Active,
                index.Baseline);

	    public Index GetInstance(Relation[] relation)
	    {
		    if (relation.Length < 2) return null;
			var result = new Index(0,relation[0].Name,null,new BaseEntity[2],int.MinValue,true,false,true,true);
		    for (var i = 0; i < 2; ++i) result.Fields[i] = relation[i];
			return result;
	    }

	    public Index GetPkInstance() =>
		    new Index(
			    0,
			    Constants.DefaultPrimaryKey.Name,
			    string.Empty,
				new BaseEntity [] { Constants.DefaultPrimaryKey },
			    0,
			    true,
			    false,
			    true,
			    true);


	}
}