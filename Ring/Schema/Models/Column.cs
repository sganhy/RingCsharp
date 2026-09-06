using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using System.Runtime.InteropServices;

namespace Ring.Schema.Models;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Column : IEquatable<Column>
{
	// Size per instance: 40 bytes as a class, 24 bytes as a struct.
	// As a struct array, a table's 30–40 columns sit in one contiguous block, so iterating them is a linear memory scan that the CPU prefetcher handles efficiently.
	internal readonly int Id;
	internal readonly int RecordIndex;
	internal readonly int BinaryLength; // Used by the drivers to determine the byte length of the value when it is a binary data type.
	internal readonly EntityType Type; // enum EntityType : byte
	internal readonly FieldType FieldType; // enum FieldType : byte
	internal readonly SearchableType SearchableType; // enum SearchableType : byte
	internal readonly bool BinaryType; // Whether the value should be stored as binary in the database (e.g., int, long, float, double, boolean) or as text (e.g., string, date, datetime, guid).
	internal readonly string PhysicalName;

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Column(EntityType entityType, FieldType fieldType, string physicalName, SearchableType searchableType, int id, int recordIndex, int binaryLength, bool binaryType)
	{
		Id = id;
		FieldType = fieldType;
		PhysicalName = physicalName;
		Type = entityType;
		RecordIndex = recordIndex;
		SearchableType = searchableType;
		BinaryType = binaryType;
		BinaryLength = binaryLength;
	}

	public static bool operator ==(in Column left, in Column right) => left.Equals(right);
	public static bool operator !=(in Column left, in Column right) => !left.Equals(right);
	public override readonly bool Equals(object? obj) => obj is Column column && Equals(column);
	public readonly bool Equals(Column other) => this.IsEquivalentTo(other);
	public override readonly int GetHashCode() => this.Hash();

#if DEBUG
	public override string ToString() => $"{Id} - {PhysicalName}";
#endif
}