using Microsoft.Extensions.Logging;
using Npgsql;
using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.PostgreSQL;

public sealed class Connection : IRingConnection, IDisposable
{
    private readonly static Dictionary<string, int> _connectionCounts = new(); // <connectionString.ToUpper(), connectionCount>
    private readonly object _syncRoot = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<Connection> _logger;
    private readonly int _id;
    private readonly DateTime _creationTime;
    private readonly static string?[] EmptyResult = Array.Empty<string?>();
    private readonly bool _informationEnabled; // logging level information enabled ?
    private NpgsqlConnection _connection;
    private DateTime _lastConnectionTime = DateTime.MinValue;
    private DateTime _lastExecutionTime = DateTime.MinValue;


    // ============ L O G S =======
    // ddl: 
    private static readonly Action<ILogger, string, Exception?> _logDdlException =
                LoggerMessage.Define<string>(LogLevel.Error, new EventId(117, nameof(LogDdlException)), "{Message}");
    private static readonly Action<ILogger, string, Exception?> _logUnsupportedOperation =
                LoggerMessage.Define<string>(LogLevel.Error, new EventId(131, nameof(LogUnSupportedOperation)), "{Message}");
    private static readonly Action<ILogger, string, Exception?> _logOperationPerformed =
                LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(LogOperationPerformed)), "{Message}");

    public Connection(IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = _configuration.LoggerFactory.CreateLogger<Connection>();
        _informationEnabled = _logger.IsEnabled(LogLevel.Information);
        _connection = new NpgsqlConnection(_configuration.ConnectionString);
        var key = _configuration.ConnectionString?.ToUpper(CultureInfo.InvariantCulture) ?? string.Empty;
        lock (_syncRoot)
        {
            if (_connectionCounts.ContainsKey(key)) ++_connectionCounts[key];
            else _connectionCounts.Add(key, 1);
            _id = _connectionCounts[key];
        }
        _creationTime = DateTime.Now;
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
        if (_informationEnabled) _lastExecutionTime = DateTime.Now;
        int returnValue;
        var sql = query.ToSql();
        if (sql==null)
        {
            LogUnSupportedOperation(query);
            return 0;
        }

        // Review SQL queries for security vulnerabilities
        // Do not catch general exception types
#pragma warning disable CA2100, CA1031
        var cmd = new NpgsqlCommand(sql, _connection);
        try
        {
            cmd.ExecuteNonQuery();
            returnValue = 1;
        }
        catch (Exception ex)
        {
            LogDdlException(ex, query);
            returnValue = 0;
        }
#pragma warning restore CA1031, CA2100
        cmd.Connection = null;
        cmd.Dispose();

        if (returnValue>0 && _informationEnabled) 
            LogOperationPerformed(query,DateTime.Now-_lastExecutionTime);

        return returnValue;
    }

    public int Execute(in SaveQuery query)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LogDdlException(Exception ex, AlterQuery query) => 
        _logDdlException(_logger, query.ToErrorMessage(ex), ex);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LogUnSupportedOperation(AlterQuery query) =>
        _logUnsupportedOperation(_logger, query.ToLogUnsupportedOperation(), null);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LogOperationPerformed(AlterQuery query, TimeSpan ts) =>
        _logOperationPerformed(_logger, query.ToLogOperationPerformed(ts), null);
}
