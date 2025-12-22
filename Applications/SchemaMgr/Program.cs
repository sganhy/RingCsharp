using Ring;
using Ring.Schema;

namespace SchemaMgr;

public static class Program
{
    public async static Task Main(string[] args)
    {
        // See https://aka.ms/new-console-template for more information
        Console.WriteLine("Hello, World!");
		Initialize.Start(string.Empty);

		var builder = new DocumentBuilder("C:/Coding/Ring/RingCsharp/Docs/rpg_schema.xml");
		//var builder = new DocumentBuilder("C:/Temp/schema/schema.xml");
        DateTime dt = DateTime.Now;
	    var document = await builder.GetDocumentAsync(DocumentType.XmlNative).ConfigureAwait(false);
		Console.WriteLine(dt - DateTime.Now);
	}
}