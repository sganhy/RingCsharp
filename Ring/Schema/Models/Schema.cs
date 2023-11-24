using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class Schema : BaseEntity
{
    internal readonly ConnectionPool Connections;
    internal readonly Lexicon[] Lexicons;   // sorted table by name (case sensitif)
    internal readonly SchemaLoadType LoadType;
    internal readonly SchemaSourceType Source;
    internal readonly Sequence[] Sequences; // sorted sequence by name (case sensitif)
    internal readonly Table[] TablesById;   // sorted table by id
    internal readonly Table[] TablesByName; // sorted table by name (case sensitif)
    internal readonly TableSpace[] TableSpaces;
    internal readonly Parameter[] Parameters;
    internal readonly DatabaseProvider Provider;

    internal Schema(int id, string name, string? description, Parameter[] parameters, Lexicon[] lexicons, SchemaLoadType loadType,
        SchemaSourceType source, Sequence[] sequences, Table[] tablesById, Table[] tablesByName, TableSpace[] tableSpaces, DatabaseProvider provider,
        bool active, bool baseline) : base(id, name, description, active, baseline)
    {
        Connections = new ConnectionPool(id, parameters.GetMinPoolSize(id), parameters.GetMaxPoolSize(id),
                                 parameters.GetDbConnectionString(id), parameters.GetDbConnectionType(id));
        Lexicons = lexicons;
        LoadType = loadType;
        Source = source;
        Sequences = sequences;
        TablesById = tablesById;
        TablesByName = tablesByName;
        TableSpaces = tableSpaces;
        Parameters = parameters;
        Provider = provider;
    }
}
