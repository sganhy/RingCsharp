// See https://aka.ms/new-console-template for more information

using Npgsql;
using Ring.Data;
using Ring.Schema.Builders;
using System;
using System.Globalization;

var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;" +
                "Host=localhost;Port=5432;Database=postgres; Pooling=false;";
var val = true;
int ok = 19;
float opop = 0.4f;
var con = new NpgsqlConnection(connectionString: "Server=localhost;Port=5432;User Id=postgres;Password=passw0rd;Database=testdb;");

TableBuilder builder = new ();

var t = builder.GetMeta("test", Ring.Schema.Enums.DatabaseProvider.PostgreSql);
var dtTest  = DateTime.ParseExact("2005-12-12T18:17:16.015+06:00", "yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
//Initialize.Start(typeof(NpgsqlConnection), POSTGRE_CONN_STRING1); +06:00

//char[] chars = new char[5];
//var testu = new string(chars);
//testu.SetBitValue(44);

var rcd = new Record();

int oi = 0;
++oi;


