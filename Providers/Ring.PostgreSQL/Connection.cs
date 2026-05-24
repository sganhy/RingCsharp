using Ring.Data;
using Ring.Data.Models;

namespace Ring.PostgreSQL;

public sealed class Connection : IConnection
{
	private int _id;
	public int Id => throw new NotImplementedException();
	public string ConnectionString => throw new NotImplementedException();
	public DateTime CreationTime => throw new NotImplementedException();
	public DateTime? LastConnectionTime => throw new NotImplementedException();
	public ConnectionState State => throw new NotImplementedException();


	public Connection()
	{ 
		
	}

	public void BeginTransaction()
	{
		throw new NotImplementedException();
	}

	public void Close()
	{
		throw new NotImplementedException();
	}

	public Task CloseAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public void Commit()
	{
		throw new NotImplementedException();
	}

	public IConnection CreateInstance(int id)
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		throw new NotImplementedException();
	}

	public string?[] Execute(in RetrieveQuery query)
	{
		throw new NotImplementedException();
	}

	public long Execute(in AlterQuery query)
	{
		throw new NotImplementedException();
	}

	public long Execute(in SaveQuery query)
	{
		throw new NotImplementedException();
	}

	public ValueTask<int> ExecuteAsync(in AlterQuery query, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public void Open()
	{
		throw new NotImplementedException();
	}

	public Task OpenAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public int ProviderId()
	{
		throw new NotImplementedException();
	}

	public void Rollback()
	{
		throw new NotImplementedException();
	}
}
