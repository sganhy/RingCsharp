using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Schema.Models;

internal sealed class Schema : BaseEntity
{
    public readonly ConnectionPool Connections;
    public readonly Lexicon[] Lexicons;   // sorted table by name (case sensitif)
    public readonly SchemaLoadType LoadType;
    public readonly SchemaSourceType Source;
    public readonly Sequence[] Sequences; // sorted sequence by name (case sensitif)
    public readonly Table[] TablesById;   // sorted table by id
    public readonly Table[] TablesByName; // sorted table by name (case sensitif)
    public readonly TableSpace[] TableSpaces;
    public readonly Parameter[] Parameters;
    public readonly DatabaseProvider Provider;

    public Schema(int id, string name, string? description, Parameter[] parameters, Lexicon[] lexicons, SchemaLoadType loadType,
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
