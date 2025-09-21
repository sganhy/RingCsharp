using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Util.Helpers;

namespace SchemaMgr;

public class Program
{
    public static async Task Main(string[] args)
    {
        // See https://aka.ms/new-console-template for more information
        Console.WriteLine("Hello, World!");

        var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;Host=localhost;Port=5432;Database=postgres; Pooling=false;";


        Meta meta = new Meta();
        Console.WriteLine(meta.Name);
        //Parser.Default


        var test = new ResourceHelper();
        Console.WriteLine("1");

        Console.WriteLine(ResourceHelper.GetErrorMessage(Ring.Util.Enums.ResourceType.RecordValueTooLarge));

        Console.WriteLine("2");

        var paramType = ResourceHelper.GetParameter(ParameterType.LastUpgrade);

        Console.WriteLine(paramType.ValueType);

        Console.WriteLine("3");

        var starTime = DateTime.Now;
        //var builder = new DocumentBuilder("C:/Coding/Ring/RingCsharp/Docs/rpg_schema.xml");
        var builder = new DocumentBuilder("C:/Temp/Schema/schema.xml");
        //var doc = await builder.GetDocumentAsync(DocumentType.XmlNative, cancellationToken);
        Console.WriteLine(DateTime.Now - starTime);
    }
}