// See https://aka.ms/new-console-template for more information

using Ring.Data;
using Ring.PostgreSQL;
using Ring.Schema;
using Ring.Schema.Builders;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;


var logger = new LoggerConfiguration()
                          // add console as logging target
                          .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                          // add a logging target for warnings and higher severity  logs
                          // structured in JSON format
                          .WriteTo.File(new JsonFormatter(), "important.json")
                          // add a rolling file for all logs
                          .WriteTo.File("all.logs",
                                        restrictedToMinimumLevel: LogEventLevel.Warning,
                                        rollingInterval: RollingInterval.Day)
                          // set default minimum level
                          .MinimumLevel.Debug()
                          .CreateLogger();

var microsoftLoggerFactory = new SerilogLoggerFactory(logger);

// logging
Log.Verbose("Some verbose log");
Log.Debug("Some debug log");
Log.Information("Person1: {@person}");
Log.Information("Car2: {@car}");
Log.Warning("Warning accrued at {now}", DateTime.Now);
Log.Error("Error accrued at {now}", DateTime.Now);
Log.Fatal("Problem with car car accrued at {now}", DateTime.Now);

SpanList<Meta> testlst = new SpanList<Meta>(3);
testlst.Add(new Meta("1"));
testlst.Add(new Meta("2"));
testlst.Add(new Meta("3"));
testlst.Add(new Meta("4"));
testlst.Add(new Meta("5"));
testlst.Add(new Meta("6"));
testlst.Add(new Meta("7"));
testlst.Add(new Meta("8"));
testlst.Add(new Meta("9"));

List<int> testh= new List<int>();
testh.Sort();



var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;Host=localhost;Port=5432;Database=postgres; Pooling=false;";

var configuration = new Configuration { ConnectionString = POSTGRE_CONN_STRING1, LoggerFactory = microsoftLoggerFactory };
IRingConnection conn = new Ring.PostgreSQL.Connection(configuration);



conn.Open();

var builder = new SchemaBuilder();
var config = new Configuration
{
    ConnectionString = POSTGRE_CONN_STRING1,
    DefaultSchema = "public",
    MinConnectionPoolSize = 1,
    MaxConnectionPoolSize = 4,
    DefaultTableStorage = "ring_data",
    DefaultIndexStorage = "ring_index"
};

var schema = builder.GetMeta(Ring.Schema.Enums.DatabaseProvider.PostgreSql, config);

BulkAlter ba = new(schema);
ba.CreateTable("@meta");
ba.CreateTable("@meta_id");
ba.CreateTable("@log");
ba.Apply(conn);

