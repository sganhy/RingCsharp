// See https://aka.ms/new-console-template for more information

using AutoFixture;
using Npgsql;
using Ring;
using Ring.Console;
using Ring.Data;
using Ring.Schema.Builders;
using Ring.Console.Extensions;
using System.Data;


var fixture = new Fixture();
var test = fixture.CreateMany<string>(1000000).ToArray();
test[1] = "111111";
Array.Sort(test);
var index = test.GetIndex("111111");

// test 1
var date = DateTime.Now;
Console.WriteLine("-- TEST 1 --");
Console.WriteLine("GetIndex(this string[] elements, string value) : ");
for (var i = 0; i < 200000; ++i)
    index = test.GetIndex("111111");

Console.WriteLine((long)(DateTime.Now-  date).TotalMilliseconds);

// test 2
date = DateTime.Now;
Console.WriteLine("-- TEST 2 --");
Console.WriteLine("GetSpanIndex(this Span<string> elements, string value) : ");
var span = test.AsSpan();
for (var i = 0; i < 200000; ++i)
    index = span.GetSpanIndex("111111");

Console.WriteLine((long)(DateTime.Now - date).TotalMilliseconds);

int oi = 0;
++oi;

/*
var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;" +
                "Host=localhost;Port=5432;Database=postgres; Pooling=false;";


{
    var ee = new TestRecord(11, true);
    ee.Id = 445;
    Record rcd22 = new Record();
    rcd22.ClearData();
}

var val = true;
int ok = 19;
float opop = 0.4f;
TimeZone localZone = TimeZone.CurrentTimeZone;
var myDateTime = DateTime.Now;
//TableBuilder builder = new ();

Initialize.Start(typeof(NpgsqlConnection), POSTGRE_CONN_STRING1);

var connection = new NpgsqlConnection(POSTGRE_CONN_STRING1);
connection.Open();
using var command = new NpgsqlCommand();
command.Connection = connection;
//command.CommandText = "select table_schema, table_name from information_schema.tables";
command.CommandText = "SELECT table_schema, table_name FROM information_schema.tables";
command.CommandType = CommandType.Text;
var param1 = new NpgsqlParameter("B1", NpgsqlTypes.NpgsqlDbType.Smallint);
param1.NpgsqlValue = (short)3;
command.Parameters.Add(param1);
//command.Parameters.Clear();
var dt = DateTime.Now;
using var reader = command.ExecuteReader();
int count = 0; 
while (reader.Read())
{
    ++count;
}
var ts = DateTime.Now - dt;
connection.Close();
Console.WriteLine(ts);
//char[] chars = new char[5];
//var testu = new string(chars);
//testu.SetBitValue(44);

var rcd = new Record();

int oi = 0;
++oi;

*/
