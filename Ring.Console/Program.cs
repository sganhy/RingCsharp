// See https://aka.ms/new-console-template for more information

using Npgsql;
using Ring.Data;
using Ring.Schema.Builders;
using Ring.Util.Extensions;

var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;" +
                "Host=localhost;Port=5432;Database=postgres; Pooling=false;";
var val = true;


var con = new NpgsqlConnection(connectionString: "Server=localhost;Port=5432;User Id=postgres;Password=passw0rd;Database=testdb;");

TableBuilder builder = new TableBuilder();

var t = builder.GetMeta("test", Ring.Schema.Enums.DatabaseProvider.PostgreSql);

//Initialize.Start(typeof(NpgsqlConnection), POSTGRE_CONN_STRING1);

//char[] chars = new char[5];
//var testu = new string(chars);
//testu.SetBitValue(44);

var rcd = new Record();

int oi = 0;
++oi;


