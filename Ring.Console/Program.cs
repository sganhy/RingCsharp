// See https://aka.ms/new-console-template for more information

using AutoFixture;
using Npgsql;
using Ring;
using Ring.Console;
using Ring.Data;
using Ring.Schema.Builders;
using Ring.Console.Extensions;
using System.Data;
using Ring.PostgreSQL;
using Ring.Schema;
using System.Drawing;
using Ring.Data.Models;


(var tes1, var test2) =   GetTest();

var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;Host=localhost;Port=5432;Database=postgres; Pooling=false;";

var configuration = new Configuration { ConnectionString = POSTGRE_CONN_STRING1 };
IRingConnection conn = new Ring.PostgreSQL.Connection(configuration);

var element = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MetaTest));

conn.Open();

var sql = "select id, skill2book, \"name\", sub_name, is_group, category, armor_penality, trained_only, try_again from public.skill";
var query = new RetrieveQuery();
query.Page = new PageInfo(12,54);

var result = conn.Execute(query);

int oi = 0;
++oi;


/*



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


(int , string ) GetTest ()

{
    return (0, "Test");
}