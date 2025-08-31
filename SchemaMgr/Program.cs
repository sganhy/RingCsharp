using CommandLine;
using Ring.Data;
using Ring.Schema;
using Ring.Schema.Attributes;
using Ring.Schema.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;Host=localhost;Port=5432;Database=postgres; Pooling=false;";

Meta meta = new Meta();
Console.WriteLine(meta.Name);
//Parser.Default


var test = new Ring.Util.Helpers.ResourceHelper();
Console.WriteLine("1");

Console.WriteLine(Ring.Util.Helpers.ResourceHelper.GetErrorMessage(Ring.Util.Enums.ResourceType.RecordValueTooLarge));

Console.WriteLine("2");

var paramType = ResourceHelper.GetParameter(ParameterType.LastUpgrade);

Console.WriteLine(paramType.ValueType);

Console.WriteLine("3");

