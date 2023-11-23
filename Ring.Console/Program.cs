// See https://aka.ms/new-console-template for more information

using Npgsql;
using Ring;
using Ring.Schema.Builders;
using Ring.Schema.Enums;



var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;" +
                "Host=localhost;Port=5432;Database=postgres; Pooling=false;";

Console.WriteLine("Hello, World!");

var con = new NpgsqlConnection(connectionString: "Server=localhost;Port=5432;User Id=postgres;Password=passw0rd;Database=testdb;");

TableBuilder builder = new TableBuilder();

var t = builder.GetMeta("test", Ring.Schema.Enums.DatabaseProvider.PostgreSql);

Initialize.Start(typeof(NpgsqlConnection), POSTGRE_CONN_STRING1);



int oi = 0;
++oi;

