using Ring.Schema.Builders;
using Ring.Schema.Enums;

namespace SchemaMgr;

public static class Program
{
    public async static Task Main(string[] args)
    {

        var te = 4 >> 1;
        ++te;
        // See https://aka.ms/new-console-template for more information
        Console.WriteLine("Hello, World!");
        var builder = new DocumentBuilder("C:/Coding/Ring/RingCsharp/Docs/rpg_schema.xml");

        var document = await builder.GetDocumentAsync(DocumentType.XmlNative).ConfigureAwait(false);




    }
}