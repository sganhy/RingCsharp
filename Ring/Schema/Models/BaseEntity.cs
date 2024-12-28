namespace Ring.Schema.Models;

internal abstract class BaseEntity
{
	internal readonly int Id;
	internal readonly string Name;
	internal readonly string PhysicalName;
	internal readonly string? Description;
	internal readonly bool Baseline;
	internal readonly bool Active;

	/// <summary>
	/// 	Ctor
	/// </summary>
	/// <param name="id">BaseEntity id</param>
	/// <param name="name">BaseEntity logical name</param>
	/// <param name="name">BaseEntity physical name</param>
	/// <param name="description">BaseEntity description</param>
	/// <param name="active">Is entity enabled</param>
	/// <param name="baseline">Is entity baselined</param>
	protected BaseEntity(int id, string name, string physicalName, string? description, bool baseline, bool active)
	{
		Id = id;
		Name = name;
		PhysicalName = physicalName;
		Description = description;
		Baseline = baseline;
		Active = active;
	}

#if DEBUG
	public override string ToString() => $"{Id} - {Name}";
#endif

}