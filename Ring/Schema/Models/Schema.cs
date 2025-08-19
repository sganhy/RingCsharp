using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Util.Builders;

namespace Ring.Schema.Models;

internal sealed class Schema : BaseEntity
{
	internal readonly ConnectionPool Connections;
	internal readonly Lexicon[] Lexicons;         // sorted table by Name (case sensitif)
	internal readonly SchemaLoadType LoadType;
	internal readonly SchemaType Type;
	internal readonly Sequence[] Sequences;       // sorted sequence by Name (case sensitif)
	internal readonly Parameter[] Parameters;
	internal readonly Table[] TablesById;         // sorted table by Id
	internal readonly Table[] TablesByName;       // sorted table by Name (case sensitif)
	internal readonly TableSpace[] TableSpaces;   // sorted tablespace by Id
	internal readonly DatabaseProvider Provider;
	internal readonly IDdlBuilder DdlBuiler;
	internal readonly IDmlBuilder DmlBuiler;
	internal readonly IDqlBuilder DqlBuiler;
	internal readonly int ObjectCount;            // table count + mtm count + view count
	internal readonly string PhysicalName;        // table count + mtm count + view count

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Schema(int id, string name, string physicalName, string? description, Parameter[] parameters, Lexicon[] lexicons, SchemaLoadType loadType,
		SchemaType type, Sequence[] sequences, Table[] tablesById, Table[] tablesByName, TableSpace[] tableSpaces, DatabaseProvider provider,
		int objectCount, bool active, bool baseline) : base(id, name, description, baseline, active)
	{
		Connections = new ConnectionPool(ConnectionPoolExtensions.GetId(null), parameters.GetMinPoolSize(id), parameters.GetMaxPoolSize(id), 0,	parameters.GetDbConnectionString(id));
		Lexicons = lexicons;
		LoadType = loadType;
		Type = type;
		Sequences = sequences;
		TablesById = tablesById;
		TablesByName = tablesByName;
		TableSpaces = tableSpaces;
		Parameters = parameters;
		Provider = provider;
		ObjectCount = objectCount;
		PhysicalName = physicalName;
		DmlBuiler = provider.GetDmlBuilder();
		DdlBuiler = provider.GetDdlBuilder();
		DqlBuiler = provider.GetDqlBuilder();
	}
}