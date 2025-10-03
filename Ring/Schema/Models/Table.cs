using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class Table : BaseEntity, IEquatable<Table>
{
	internal readonly int ObjectIndex;
	internal readonly bool Cached;
	internal readonly Field[] Fields; // sorted by name.
	internal readonly Relation[] Relations; // sorted by name.
	internal readonly Index[] Indexes;
	internal readonly int RecordSize;
	internal readonly Column[] Columns;			// mix Fields and Relations.
	internal readonly PhysicalType PhysicalType;
	internal readonly int SchemaId;
	internal readonly string? Subject;
	internal readonly TableType Type;
	internal readonly CacheId CacheId;
	internal readonly string PhysicalName;
	internal readonly bool AllowHardDeletion;
	internal readonly bool Readonly;
	internal readonly bool UsePreparedStatement;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Table(int id, string name, string? description, string? subject, string physicalName, TableType type,
		Relation[] relations, Field[] fields, Column[] columns, Index[] indexes, int schemaId, PhysicalType physicalType, 
		int objectIndex, int recordSize, bool baseline, bool active, bool cached, bool allowHardDeletion, bool readonlyTable, 
		bool usePreparedStatement) : base(id, name, description, baseline, active)
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
		PhysicalType = physicalType;
		Cached = cached;
		PhysicalName = physicalName;
		ObjectIndex = objectIndex;
		AllowHardDeletion = allowHardDeletion;
		UsePreparedStatement = usePreparedStatement;
	}

	public static bool operator ==(Table left, Table right) => left.Equals(right);
	public static bool operator !=(Table left, Table right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is Table table && Equals(table);
	public bool Equals(Table? other) => this.IsEquivalentTo(other);
	public override int GetHashCode() => this.Hash();

#if DEBUG
	public sealed override string ToString() => $"{Id} - {Name} ({ObjectIndex})";
#endif

}