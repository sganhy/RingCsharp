using CommandLine;
using Ring.Data;
using Ring.Schema;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;Host=localhost;Port=5432;Database=postgres; Pooling=false;";

Meta meta = new Meta();
Console.WriteLine(meta.Name);
//Parser.Default
