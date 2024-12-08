using Npgsql;
using Microsoft.Extensions.Logging;
using Ring.Data;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Util.Builders;
using System.Data;
using System.Globalization;
using Ring.Util.Enums;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Ring.Data.Enums;
using Ring.Schema.Models;
using NpgsqlTypes;
using Ring.PostgreSQL.Extensions;
using Ring.Schema.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System;

namespace Ring.PostgreSQL;

public sealed class Connection : IRingConnection
{
    private readonly static Dictionary<string, int> _connectionCounts = new(); // <connectionString.ToUpper(), connectionCount>
    private readonly static string ActionMessage = "{Message}";
    private readonly static NpgsqlParameter [] DefaultParameterArray = Array.Empty<NpgsqlParameter>();
    private readonly static CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    private readonly static LogBuilder _logBuilder = new();
    private readonly static string?[] EmptyResult = Array.Empty<string?>();
    private readonly static object _syncRoot = new();
    private readonly static string _ddlOperationType = nameof(AlterQueryType);
    private readonly static int BindVariableNameCacheSize = 1024;
    private readonly static string[] BindVariableName = GetBindVariable();
    private const string BindVariablePrefix = "p";
    private readonly static string BooleanTrue = true.ToString(DefaultCulture);
    private readonly IConfiguration _configuration;
    private readonly ILogger<Connection> _logger;
    private readonly int _id;
    private readonly DateTime _creationTime;
    private readonly bool _informationEnabled; // logging level information enabled ?
    private NpgsqlConnection _connection;
    private DateTime _lastConnectionTime = DateTime.MinValue;
    private DateTime _lastExecutionTime = DateTime.MinValue;

    // ============ L O G S =======
    // ddl: 
    private static readonly Action<ILogger, string, Exception?> _logDdlException =
                LoggerMessage.Define<string>(LogLevel.Error, new EventId((int)EventType.DdlException, 
                    nameof(LogDdlException)), ActionMessage);
    private static readonly Action<ILogger, string, Exception?> _logOperationPerformed =
                LoggerMessage.Define<string>(LogLevel.Information, new EventId((int)EventType.QueryPerformed, 
                    nameof(LogOperationPerformed)), ActionMessage);

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


    public long Execute(in AlterQuery query)
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

        if (returnValue>0 && _informationEnabled) LogOperationPerformed(query,DateTime.Now-_lastExecutionTime);

        return returnValue;
    }

    public ValueTask<int> ExecuteAsync(in AlterQuery query, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return new(Task.FromCanceled<int>(cancellationToken));
        
        if (_informationEnabled) _lastExecutionTime = DateTime.Now;
        int returnValue;
        var sql = query.ToSql();
        if (sql == null)
        {
            LogUnSupportedOperation(query);
            return new(Task.FromResult(0));
        }
#pragma warning disable CA2100, CA1031

        var cmd = new NpgsqlCommand(sql, _connection);
        try
        {
            cmd.ExecuteNonQuery();
            returnValue = 1;
        }
        catch (OperationCanceledException e)
        {
            // warn cancelled 
            cmd.Connection = null;
            cmd.Dispose();
            return new(Task.FromCanceled<int>(e.CancellationToken));
        }
        catch (Exception ex)
        {
            LogDdlException(ex, query);
            returnValue = 0;
        }
#pragma warning restore CA1031, CA2100
        cmd.Connection = null;
        cmd.Dispose();

        if (returnValue > 0 && _informationEnabled) LogOperationPerformed(query, DateTime.Now - _lastExecutionTime);
        return new(Task.FromResult(returnValue));
    }

    public long Execute(in SaveQuery query)
    {
        //if (_informationEnabled) _lastExecutionTime = DateTime.Now;
        int returnValue;
        var sql = query.ToSql();
        if (sql == null)
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
            cmd.Parameters.AddRange(Getparameters(query));
            cmd.ExecuteNonQuery();
            returnValue = 1;
        }
        catch (Exception ex)
        {
            LogDmlException(ex, query);
returnValue = 0;
        }
#pragma warning restore CA1031, CA2100
        cmd.Connection = null;
cmd.Dispose();

        //if (returnValue > 0 && _informationEnabled) LogOperationPerformed(query, DateTime.Now - _lastExecutionTime);
        return returnValue;
    }

    #region private methods 

    private static string[] GetBindVariable()
    {
        var result = new string[BindVariableNameCacheSize];
        var count = BindVariableNameCacheSize;
        for (var i = 0; i < count; ++i) result[i] = BindVariablePrefix + (i + 1).ToString(DefaultCulture);
        return result;
    }

    private static NpgsqlParameter[] Getparameters(in SaveQuery saveQuery)
    {
        NpgsqlParameter[] result = DefaultParameterArray;
        var bindVariableNameCacheSize = BindVariableNameCacheSize;

        if (saveQuery.Type == SaveQueryType.InsertRecord)
        {
            // use an array pool
            var span = new ReadOnlySpan<IColumn>(saveQuery.Table.Columns);
            var spanCount = span.Length;
            var recordIndexes = saveQuery.Table.RecordIndexes;
            var data = saveQuery.Data;

            result = new NpgsqlParameter[spanCount];
            for (var i=0; i< spanCount; ++i)
            {
                var column = span[i];
                var value = data[recordIndexes[i]];
                var variableName = i < bindVariableNameCacheSize ? BindVariableName[i] : BindVariablePrefix + (i + 1).ToString(DefaultCulture);
                var dbType = column.FieldType.ToNpgsqlDbType();

                if (value == null)
                {
                    result[i] = new NpgsqlParameter(variableName, dbType)
                    {
                        Value = DBNull.Value
                    };
                    continue;
                }
                switch (column.FieldType)
                {
                    case FieldType.Long:
                    case FieldType.Int:
                    case FieldType.Short:
                    case FieldType.Byte:
                        result[i] = new NpgsqlParameter<long>(variableName, dbType)
                        {
                            Value = long.Parse(value, DefaultCulture)
                        };
                        break;
                    case FieldType.String:
                    case FieldType.LongString:
                        result[i] = new NpgsqlParameter<string>(variableName, dbType)
                        {
                            Value = value,
                        };
                        break;
                    case FieldType.Boolean:
                        result[i] = new NpgsqlParameter<bool>(variableName, dbType)
                        {
                            Value = string.Equals(BooleanTrue, value, StringComparison.Ordinal)
                        };
                        break;
                }
                
            }
        }
        else if (saveQuery.Type == SaveQueryType.UpdateRecord)
        {

        }
        else if (saveQuery.Type == SaveQueryType.UpdateReturningRecord)
        { 

        }
        return result;
    }

    private void LogDdlException(Exception ex, AlterQuery query) => 
        _logDdlException(_logger, _logBuilder.GetMessage(query, EventType.DdlException), ex);

    private void LogDmlException(Exception ex, SaveQuery query) 
    { 
    }

    private void LogUnSupportedOperation(AlterQuery query) {
        var message = _logBuilder.GetMessage(EventType.UnsupportedOperation,
            (int)query.Type, _ddlOperationType, query.Type.ToString());
        var ex = new ArgumentException(message);
        LogDdlException(ex, query);
    }
    private void LogUnSupportedOperation(SaveQuery query)
    {
        var message = _logBuilder.GetMessage(EventType.UnsupportedOperation,
            (int)query.Type, _ddlOperationType, query.Type.ToString());
        var ex = new ArgumentException(message);
        //LogDdlException(ex, query);
    }

    private void LogOperationPerformed(AlterQuery query, TimeSpan ts) =>
        _logOperationPerformed(_logger, string.Empty, null);

    private void LogOperationPerformed(SaveQuery query, TimeSpan ts) =>
        _logOperationPerformed(_logger, string.Empty, null);

    #endregion 
}
