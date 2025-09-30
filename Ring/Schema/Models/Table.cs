using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Table : BaseEntity, IEquatable<Table>
{
	internal readonly int ObjectIndex;
	internal readonly bool Cached;
	internal readonly Field[] Fields;              // sorted by name.
	internal readonly Relation[] Relations;        // sorted by name.
	internal readonly Index[] Indexes;
	internal readonly int RecordSize;
	internal readonly Column[] Columns;            // mix Fields and Relations.
	internal readonly PhysicalType PhysicalType;
	internal readonly int SchemaId;
	internal readonly string? Subject;
	internal readonly TableType Type;
	internal readonly CacheId CacheId;
	internal readonly string PhysicalName;
	internal readonly bool PhysicalDeletion;
	internal readonly bool Readonly;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Table(int id, string name, string? description, string? subject, string physicalName, TableType type,
		Relation[] relations, Field[] fields, Column[] columns, Index[] indexes, int schemaId,
		PhysicalType physicalType, int objectIndex, int recordSize, bool baseline, bool active, bool cached, bool physicalDeletion, bool readonlyTable) 
		: base(id, name, description, baseline, active)
	{
		Type = type;
		Fields = fields;
		RecordSize = recordSize;
		Columns = columns;
		Relations = relations;
		Indexes = indexes;
		Readonly = readonlyTable;
		Subject = subject;
		CacheId = new CacheId();
		SchemaId = schemaId;
		ObjectIndex = 0;
		PhysicalType = physicalType;
		Cached = cached;
		PhysicalName = physicalName;
		ObjectIndex = objectIndex;
		PhysicalDeletion = physicalDeletion;
	}

    public bool Equals(Table? other)
    {
        throw new NotImplementedException();
    }

#if DEBUG
	public sealed override string ToString() => $"{Id} - {Name} ({ObjectIndex})";
#endif

}