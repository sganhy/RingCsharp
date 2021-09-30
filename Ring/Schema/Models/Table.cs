using Ring.Schema.Enums;

namespace Ring.Schema.Models
{
	internal sealed class Table : BaseEntity
	{
		internal readonly bool Cached;
		internal readonly Field[] Fields;
		internal readonly Field[] FieldsById; // Could be null and bigger than Fields array
		internal readonly Index[] Indexes;
		internal readonly LexiconIndex[] LexiconIndexes; // Sorted by item1; contains <fieldId, lexiconIndex>
		internal readonly string PhysicalName;
		internal readonly PhysicalType PhysicalType;
		internal readonly Field PrimaryKey;
		internal readonly int PrimaryKeyIdIndex;
		internal readonly bool Readonly;
		internal readonly Relation[] Relations;
		internal readonly int SchemaId;
		internal readonly string Subject;
		internal readonly TableType Type;
		internal readonly bool Unlogged;
		internal CacheId CacheId;

		/// <summary>
		///     Ctor
		/// </summary>
		internal Table(int id, string name, string description, string subject, string physicalName, TableType type,
			Relation[] relations, Field[] fields, Field[] fieldsById, Index[] indexes, Field primaryKey, int schemaId,
			int primaryKeyIdIndex, LexiconIndex[] lexiconIndexes, PhysicalType physicalType, bool unlogged,
			bool baseline,
			bool active,
			bool cached, bool readonlyTable, CacheId cacheId)
			: base(id, name, description, active, baseline)
		{
			Type = type;
			Fields = fields;
			FieldsById = fieldsById;
			Relations = relations;
			Indexes = indexes;
			Readonly = readonlyTable;
			PrimaryKey = primaryKey;
			Subject = subject;
			CacheId = cacheId;
			SchemaId = schemaId;
			PhysicalName = physicalName;
			PrimaryKeyIdIndex = primaryKeyIdIndex;
			LexiconIndexes = lexiconIndexes;
			PhysicalType = physicalType;
			Cached = cached;
			Unlogged = unlogged;
		}
	}
}