// See https://aka.ms/new-console-template for more information

using Ring.Data;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using Ring.Schema.Extensions;
using System.Diagnostics;

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

// call difference trougth interface and and base class
var builder = new SchemaBuilder();
var config = new Configuration() { DefaultSchema = "public", MaxConnectionPoolSize = 20 };
var schema = builder.GetMeta(DatabaseProvider.PostgreSql, config);
var metaTable = schema.GetTable("@meta");
//var lexiconTable = schema.GetTable("@lexicon");

List<int> testh= new List<int>();
testh.Sort();

var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;Host=localhost;Port=5432;Database=postgres; Pooling=false;";

var configuration = new Configuration { ConnectionString = POSTGRE_CONN_STRING1, LoggerFactory = microsoftLoggerFactory };
IRingConnection conn = new Ring.PostgreSQL.Connection(configuration);
conn.Open();

Process proc = Process.GetCurrentProcess();
Console.WriteLine("proc.PrivateMemorySize64=" + ((double)proc.PrivateMemorySize64) / (1024 * 1024) + " MB");

/*
BulkAlter ba = new(schema);
ba.CreateTable("@log");
ba.Apply(conn);
*/

BulkSave bs = new(schema);

for (var i = 0; i < 10_000; ++i)
{
    var rcd = new Record(metaTable);
    rcd.SetField("id", i);
    rcd.SetField("schema_id", 1);
    rcd.SetField("object_type", 0);
    rcd.SetField("reference_id", 0);
    rcd.SetField("data_type", 332);
    rcd.SetField("flags", -332784545);
    rcd.SetField("name", "test");
    //rcd.SetField("description", "test desc");
    rcd.SetField("value", "test value");
    rcd.SetField("active", true);
    bs.InsertRecord(rcd);
}
var checkTime = DateTime.Now;
Console.WriteLine("Start - bs.Save(conn);");
bs.Save(conn, true);
Console.WriteLine("End - " + (DateTime.Now - checkTime));

proc = Process.GetCurrentProcess();
Console.WriteLine("proc.PrivateMemorySize64=" + ((double)proc.PrivateMemorySize64)/(1024*1024) + " MB");
Console.WriteLine("Version test 3");

int oi = 0;
++oi;