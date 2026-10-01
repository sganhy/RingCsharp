namespace Ring.Data.Models;

internal readonly struct AlterQueryComparer : IComparer<AlterQuery>
{
	public static int Compare(in AlterQuery x, in AlterQuery y)
	{
		// Code size: 44 (0x2c)
		int typeComp = ((byte)x.Type).CompareTo((byte)y.Type);
		return typeComp != 0 ? typeComp : x.Id.CompareTo(y.Id);
	}

	// Explicit interface implementation to satisfy IComparer<AlterQuery>
	int IComparer<AlterQuery>.Compare(AlterQuery x, AlterQuery y) => Compare(in x, in y);
}