using Npgsql;
using Ring.Data;
using System.Data;
using System.Globalization;

namespace Ring.PostgreSQL;

public sealed class Connection : IRingConnection, IDisposable
{
    private readonly static Dictionary<string, int> _connectionCounts = new(); // <connectionString.ToUpper(), connectionCount>
    private readonly object _syncRoot = new();
    private readonly IConfiguration _configuration;
    private readonly int _id;
    private readonly DateTime _creationTime; 
    private NpgsqlConnection _connection;
    private DateTime? _lastConnectionTime;

    public Connection(IConfiguration configuration)
    {
        _configuration = configuration;
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
        
        using (var cmd = new NpgsqlCommand(sql, _connection))
        using (var reader = cmd.ExecuteReader())
        {
            if (!reader.HasRows) return Array.Empty<string?>();
            int i = 0;
            while (reader.Read())
            {
                for (i=0; i < columnCount; ++i)
                {
                    var val = reader.GetValue(i);
                    if (val is DBNull) result.Add(null);
                    else if (val is string) result.Add(val as string);
                    else result.Add(reader.GetValue(i).ToString());
                }
            }
            cmd.Connection = null;
        }
        return result.ToArray();
    }

}
