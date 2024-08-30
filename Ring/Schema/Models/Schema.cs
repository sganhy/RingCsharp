﻿using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Util.Builders;

namespace Ring.Schema.Models;

internal sealed class Schema : BaseEntity
{
	internal readonly ConnectionPool Connections;
	internal readonly Lexicon[] Lexicons;         // sorted table by name (case sensitif)
	internal readonly SchemaLoadType LoadType;
	internal readonly SchemaType Type;
	internal readonly Sequence[] Sequences;       // sorted sequence by name (case sensitif)
	internal readonly Parameter[] Parameters;
	internal readonly Table[] TablesById;         // sorted table by id
	internal readonly Table[] TablesByName;       // sorted table by name (case sensitif)
	internal readonly TableSpace[] TableSpaces;
	internal readonly DatabaseProvider Provider;
	internal readonly IDdlBuilder DdlBuiler;
	internal readonly IDmlBuilder DmlBuiler;
	internal readonly IDqlBuilder DqlBuiler;

	internal Schema(int id, string name, string? description, Parameter[] parameters, Lexicon[] lexicons, SchemaLoadType loadType,
        SchemaType type, Sequence[] sequences, Table[] tablesById, Table[] tablesByName, TableSpace[] tableSpaces, DatabaseProvider provider,
		bool active, bool baseline) : base(id, name, description, active, baseline)
	{
		Connections = new ConnectionPool(id, parameters.GetMinPoolSize(id), parameters.GetMaxPoolSize(id),
                                 parameters.GetDbConnectionString(id));
		Lexicons = lexicons;
		LoadType = loadType;
		Type = type;
		Sequences = sequences;
		TablesById = tablesById;
		TablesByName = tablesByName;
		TableSpaces = tableSpaces;
		Parameters = parameters;
		Provider = provider;
		DmlBuiler = provider.GetDmlBuilder();
		DdlBuiler = provider.GetDdlBuilder();
		DqlBuiler = provider.GetDqlBuilder();
	}
}