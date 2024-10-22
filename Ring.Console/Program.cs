// See https://aka.ms/new-console-template for more information

using Ring.Data;
using Ring.Data.Models;
using Ring.PostgreSQL;
using Ring.Schema.Builders;
using Ring.Schema.Extensions;
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

var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;Host=localhost;Port=5432;Database=postgres; Pooling=false;";

var configuration = new Configuration { ConnectionString = POSTGRE_CONN_STRING1, LoggerFactory = microsoftLoggerFactory };
IRingConnection conn = new Ring.PostgreSQL.Connection(configuration);

conn.Open();

var builder = new SchemaBuilder();
var schema = builder.GetMeta("public", Ring.Schema.Enums.DatabaseProvider.PostgreSql, 1, POSTGRE_CONN_STRING1);
var metaTable = schema.GetTable("@meta");
var metaIdTable = schema.GetTable("@meta_id");
var query1 = new AlterQuery(metaTable, Ring.Data.Enums.AlterQueryType.CreateTable, schema.DdlBuiler);
var query2 = new AlterQuery(metaIdTable, Ring.Data.Enums.AlterQueryType.CreateTable, schema.DdlBuiler);

var result = conn.Execute(query1);
result = conn.Execute(query2);