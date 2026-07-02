using Ring.Schema;

namespace SchemaMgr;

public static class Program
{
    public async static Task Main(string[] args)
    {
        // See https://aka.ms/new-console-template for more information
        Console.WriteLine("Hello, World!");


		var POSTGRE_CONN_STRING1 = "User ID=postgres; Password=sa;Host=localhost;Port=5432;Database=postgres; Pooling=false;";
		var connection = new Ring.PostgreSQL.Connection(POSTGRE_CONN_STRING1);

		connection.Open();

		/*
		Runtime.Start(string.Empty);

		// Sync
		Engine.Configure()
			  .WithConnectionString("Host=localhost;Database=mydb;Username=app;Password=secret")
			  .WithProvider(DatabaseProvider.PostgreSql)
			  .WithPoolSize(2, 16)
			  .Start();

		// Async
		await Engine.Configure()
					.WithConnectionString(connString)
					.WithProvider(DatabaseProvider.PostgreSql)
					.WithPoolSize(2, 16)
					.StartAsync(cancellationToken);
		*/
		var builder = new DocumentBuilder("C:/Coding/Ring/RingCsharp/Docs/rpg_schema.xml");
		//var builder = new DocumentBuilder("C:/Temp/schema/schema.xml");
        DateTime dt = DateTime.Now;
	    var document = await builder.GetDocumentAsync(DocumentType.XmlNative).ConfigureAwait(false);
		Console.WriteLine(dt - DateTime.Now);
	}
}