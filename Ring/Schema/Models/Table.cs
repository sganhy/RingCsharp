using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Table : BaseEntity
{
	internal readonly bool Cached;
	internal readonly Field[] Fields;         // sorted by name
	internal readonly Field[] FieldsById;     // sorted by id
	internal readonly Relation[] Relations;   // sorted by name
	internal readonly Index[] Indexes;        
	internal readonly string PhysicalName;
	internal readonly PhysicalType PhysicalType;
	internal readonly int SchemaId;
	internal readonly string? Subject;
	internal readonly TableType Type;
	internal readonly CacheId CacheId;
	internal readonly bool Readonly;

    /// <summary>
    ///     Ctor
    /// </summary>
	internal Table(int id, string name, string? description, string? subject, string physicalName, TableType type,
		Relation[] relations, Field[] fields, Field[] fieldsById, Index[] indexes, int schemaId, PhysicalType physicalType, 
		bool baseline, bool active, bool cached, bool readonlyTable) : base(id, name, description, active, baseline)
	{
		Type = type;
		Fields = fields;
		FieldsById = fieldsById;
		Relations = relations;
		Indexes = indexes;
		Readonly = readonlyTable;
		Subject = subject;
		CacheId = new CacheId();
		SchemaId = schemaId;
		PhysicalName = physicalName;
		PhysicalType = physicalType;
		Cached = cached;
	}
}
