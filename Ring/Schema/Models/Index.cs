namespace Ring.Schema.Models;

internal sealed class Index : BaseEntity
{
    internal readonly bool Bitmap;
    internal readonly string[] Columns;
    internal readonly bool Unique;

    /// <summary>
    ///     Ctor
    /// </summary>
    internal Index(int id, string name, string? description, string[] columns, bool unique, bool bitmap, bool active, bool baseline)
        : base(id, name, description, active, baseline)
    {
        Unique = unique;
        Columns = columns;
        Bitmap = bitmap;
    }
}