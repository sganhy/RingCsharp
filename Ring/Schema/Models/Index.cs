namespace Ring.Schema.Models;

internal sealed class Index : BaseEntity
{
	internal readonly bool Bitmap;
	internal readonly string[] Columns;
	internal readonly bool Unique;
    internal readonly string PhysicalName;

    /// <summary>
    /// 	Ctor
    /// </summary>
    internal Index(int id, string name, string physicalName, string? description, string[] columns, bool unique, bool bitmap, bool active, bool baseline)
		: base(id, name, description, baseline, active)
	{
		Unique = unique;
		Columns = columns;
		Bitmap = bitmap;
		PhysicalName = physicalName;
    }
}