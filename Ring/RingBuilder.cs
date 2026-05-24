namespace Ring;

public sealed class RingBuilder
{
	private string _connectionString = string.Empty;
	private string _provider = string.Empty;
	private int _maxSchemas = 16;
	private int _minPoolSize = 2;
	private int _maxPoolSize = 16;

	internal RingBuilder() { }

	public RingBuilder WithConnectionString(string connectionString)
	{
		_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
		return this;
	}

	public RingBuilder WithProvider(string provider)
	{
		_provider = provider;
		return this;
	}

	public RingBuilder WithMaxSchemas(int max)
	{
		_maxSchemas = max;
		return this;
	}

	public RingBuilder WithMinPoolSize(int min)
	{
		_minPoolSize = min;
		return this;
	}

	public RingBuilder WithMaxPoolSize(int max)
	{
		_maxPoolSize = max;
		return this;
	}

	public RingBuilder WithPoolSize(int min, int max)
	{
		_minPoolSize = min;
		_maxPoolSize = max;
		return this;
	}

	/// <summary>Boots the Ring engine synchronously.</summary>
	public void Start()
	{
		//Initialize.Start(BuildOptions());
	}

	/// <summary>Boots the Ring engine asynchronously.</summary>
	public Task StartAsync(CancellationToken cancellationToken = default)
	{
		//RunTime.StartAsync(BuildOptions(), cancellationToken);
		throw new NotImplementedException();
	}
		
	/*
	private RingOptions BuildOptions() => new()
	{
		ConnectionString = _connectionString,
		Provider = _provider,
		MaxSchemas = _maxSchemas,
		MinPoolSize = _minPoolSize,
		MaxPoolSize = _maxPoolSize,
	};
	*/
}
