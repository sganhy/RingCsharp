using Microsoft.Extensions.Logging;
using Npgsql;
using Ring.Data;
using Ring.Data.Enums;
using Ring.Data.Models;
using System.Data;
using System.Globalization;

namespace Ring.PostgreSQL;

public sealed class Connection : IRingConnection, IDisposable
{
    private readonly static Dictionary<string, int> _connectionCounts = new(); // <connectionString.ToUpper(), connectionCount>
    private readonly object _syncRoot = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly int _id;
    private readonly DateTime _creationTime;
    private readonly static string?[] EmptyResult = Array.Empty<string?>();
    private readonly bool _informationEnabled; // logging level information enabled ?
    private NpgsqlConnection _connection;
    private DateTime? _lastConnectionTime;

    public Connection(IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = _configuration.Logger;
        _informationEnabled = _configuration.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information);
        _connection = new NpgsqlConnection(_configuration.ConnectionString);
        var key = _configuration.ConnectionString?.ToUpper(CultureInfo.InvariantCulture) ?? string.Empty;
        lock (_syncRoot)
        {
            if (_connectionCounts.ContainsKey(key)) ++_connectionCounts[key];
            else _connectionCounts.Add(key, 1);
            _id = _connectionCounts[key];
        }
        _creationTime = DateTime.Now;
        _lastConnectionTime = null;
    }

    public string ConnectionString => _configuration.ConnectionString;

    public DateTime CreationTime => _creationTime;

    public DateTime? LastConnectionTime => _lastConnectionTime;

    public ConnectionState State {
        get
        {
            // check connection at this time !!
            return _connection.State;
        }
    }

    public int Id => _id;

    public void Open()
    {
        _connection.Open();
        _lastConnectionTime = DateTime.Now;
    }

    public Task OpenAsync(CancellationToken cancellationToken)
    {
        var task = _connection.OpenAsync(cancellationToken);
        _lastConnectionTime = DateTime.Now;
        return task;
    }

    public void Close()
    {
        _connection.Close();
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await _connection.CloseAsync().ConfigureAwait(false);
    }

    public IRingConnection CreateNewInstance() => new Connection(_configuration);

    public void Dispose()
    {
        _connection?.Dispose();
    }

    public Span<string?> ExecuteSelect(string sql, int columnCount, Span<(string, byte)> parameters)
    {
        var result = new List<string?>();
        NpgsqlCommand? cmd=null;
        NpgsqlDataReader? reader = null;
        try
        {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            cmd = new(sql, _connection);
#pragma warning restore CA2100
            reader = cmd.ExecuteReader();
            if (!reader.HasRows) return Array.Empty<string?>();
            int i = 0;
            while (reader.Read())
            {
                for (i = 0; i < columnCount; ++i)
                {
                    var v = reader.GetValue(i);
                    if (v is DBNull) result.Add(null);
                    else if (v is string) result.Add(v as string);
                    else result.Add(v.ToString());
                }
            }
        }
        finally
        {
            reader?.Close();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            cmd.Connection = null;
#pragma warning restore CS8602
            cmd.Dispose();
            reader?.Dispose();
        }
        return result.ToArray();
    }

    public string?[] Execute(in RetrieveQuery query)
    {
        query.Page.Count = 521;
        return EmptyResult;
    }


    public int Execute(in AlterQuery query)
    {
        int returnValue;
        string? sql = query.Type switch
        {
            AlterQueryType.CreateTable => query.Builder.Create(query.Table),
            _ => null
        };

        if (sql==null)
        {
            LoggerExtensions.LogError(_logger, "Not supported AlterQueryType #{}", (int)query.Type);
            return 0;
        }

        NpgsqlCommand? cmd = null;
        try
        {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            cmd = new(sql, _connection);
            cmd.ExecuteNonQuery();
#pragma warning restore CA2100
            returnValue = 1;
        }
        // Do not catch general exception types
#pragma warning disable CA1031
        catch (Exception ex)
        {
            LoggerExtensions.LogError(_logger, ex, sql);
            returnValue = 0;
        }
#pragma warning restore CA1031 

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        cmd.Connection = null;
#pragma warning restore CS8602
        cmd.Dispose();
        
        return returnValue;
    }

    public int Execute(in SaveQuery query)
    {
        throw new NotImplementedException();
    }
}
