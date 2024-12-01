using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Table : BaseEntity
{
	internal int ObjectIndex { get; private set; } // position of the table in the table collection sorted by name included MTM one.
	internal readonly bool Cached;
	internal readonly Field[] Fields;              // sorted by name (field.id should b).
	internal readonly Relation[] Relations;        // sorted by name.
	internal readonly Index[] Indexes;
	internal readonly int[] RecordIndexes;         // [tableId] <= position into Fields & Relations sorted by Column.Id.
	internal readonly int RecordSize;
	internal readonly IColumn[] Columns;           // columns[n].id should be unique.
	internal readonly string PhysicalName;
	internal readonly PhysicalType PhysicalType;
	internal readonly int SchemaId;
	internal readonly string? Subject;
	internal readonly TableType Type;
	internal readonly CacheId CacheId;
	internal readonly bool Readonly;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Table(int id, string name, string? description, string? subject, string physicalName, TableType type,
		Relation[] relations, Field[] fields, int [] recordIndexes, IColumn[] columns, Index[] indexes, int schemaId, 
		PhysicalType physicalType, bool baseline, bool active, bool cached, bool readonlyTable) : base(id, name, description, active, baseline)
	{
		Type = type;
		Fields = fields;
		RecordSize = recordIndexes.Length + 1;
		RecordIndexes = recordIndexes;
		Columns = columns;
		Relations = relations;
		Indexes = indexes;
		Readonly = readonlyTable;
		Subject = subject;
		CacheId = new CacheId();
		SchemaId = schemaId;
		ObjectIndex = -1;
		PhysicalName = physicalName;
		PhysicalType = physicalType;
		Cached = cached;
	}

	/// <summary>
	/// 	Assign only once the property
	/// </summary>
	internal void SetObjectIndex(int objectIndex) => ObjectIndex = ObjectIndex<0 ? objectIndex : ObjectIndex;

#if DEBUG
	public override string ToString() => $"{Id} - {Name} ({ObjectIndex})";
#endif

}