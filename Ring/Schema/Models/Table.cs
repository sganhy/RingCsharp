using Ring.Schema.Enums;

namespace Ring.Schema.Models;

internal sealed class Table : BaseEntity
{
	internal readonly int ObjectIndex;
	internal readonly bool Cached;
	internal readonly Field[] Fields;              // sorted by name (field.id should b).
	internal readonly Relation[] Relations;        // sorted by name.
	internal readonly Index[] Indexes;
	internal readonly int[] RecordIndexes;         // [tableId] <= position into Fields & Relations sorted by Column.Id.
	internal readonly int RecordSize;
	internal readonly IColumn[] Columns;           // mix Fields and Relations.
	internal readonly PhysicalType PhysicalType;
	internal readonly int SchemaId;
	internal readonly string? Subject;
	internal readonly TableType Type;
	internal readonly CacheId CacheId;
	internal readonly bool Readonly;
	internal readonly int ColumnCount;             // physical columns count

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Table(int id, string name, string? description, string? subject, string physicalName, TableType type,
		Relation[] relations, Field[] fields, int [] recordIndexes, IColumn[] columns, Index[] indexes, int schemaId,
		PhysicalType physicalType, int objectIndex, int columnCount, bool baseline, bool active, bool cached, bool readonlyTable) 
		: base(id, name, physicalName, description, baseline, active)
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
		ObjectIndex = 0;
		PhysicalType = physicalType;
		Cached = cached;
		ObjectIndex = objectIndex;
		ColumnCount = columnCount;
	}

#if DEBUG
	public override string ToString() => $"{Id} - {Name} ({ObjectIndex})";
#endif

}