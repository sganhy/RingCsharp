using Ring.Data;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;

namespace Ring;


public class RingOptions
{
	public string ConnectionString { get; set; } = string.Empty;
	public string Provider { get; set; } = string.Empty;
	public int MaxSchemas { get; set; } = 16;
	public int MinPoolSize { get; set; } = 2;
	public int MaxPoolSize { get; set; } = 16;
}

public static class Runtime
{
	private static int _state; // 0 = idle, 1 = starting, 2 = started  (Interlocked)

	// ── Module initializer ─────────────────────────────────────────────────────

	/// <summary>
	/// Called automatically by the CLR before any type in the assembly is first used.
	/// Wires up any static infrastructure that must exist before user code runs.
	/// </summary>
	[ModuleInitializer]
	internal static void ModuleInit()
	{
		// Guarantee Global's static constructor runs before any other
		// code in the assembly touches it — prevents a rare race when
		// multiple threads first access the assembly concurrently.
		RuntimeHelpers.RunClassConstructor(typeof(Global).TypeHandle);
	}

	// ── Public API ─────────────────────────────────────────────────────────────

	/// <summary>Returns a new fluent builder to configure the Ring engine.</summary>
	public static RingBuilder Configure() => new();

	/// <summary>
	/// <c>true</c> once <see cref="Start"/> or <see cref="StartAsync"/> has
	/// completed successfully.
	/// </summary>
	public static bool IsStarted => Volatile.Read(ref _state) == 2;

	// ── Internal entry points (called by RingBuilder) ──────────────────────────

	internal static void Start(RingOptions options)
	{
		EnsureNotStarted();
		if (Interlocked.CompareExchange(ref _state, 1, 0) != 0)
			throw new InvalidOperationException("Ring engine is already starting or has started.");
		try
		{
			Boot(options);
			Volatile.Write(ref _state, 2);
		}
		catch
		{
			Volatile.Write(ref _state, 0); // allow retry on failure
			throw;
		}
	}

	internal static async Task StartAsync(RingOptions options, CancellationToken cancellationToken = default)
	{
		EnsureNotStarted();
		if (Interlocked.CompareExchange(ref _state, 1, 0) != 0)
			throw new InvalidOperationException("Ring engine is already starting or has started.");
		try
		{
			await BootAsync(options, cancellationToken).ConfigureAwait(false);
			Volatile.Write(ref _state, 2);
		}
		catch
		{
			Volatile.Write(ref _state, 0);
			throw;
		}
	}

	// ── Boot logic ─────────────────────────────────────────────────────────────

	private static void Boot(RingOptions options)
	{
		ValidateOptions(options);
		var config = BuildConfiguration(options);
		Global.Init(config);
		/*
		var connection = ConnectionFactory.Create(options.Provider, 0, config);
		connection.Open();
		var pool = new ConnectionPool(
			Global.GetNextPoolId(),
			options.MinPoolSize,
			options.MaxPoolSize,
			resizeCount: 0,
			options.ConnectionString);
		pool.Init(connection);
		Global.SetConnectionPool(pool);
		*/
	}

	private static async Task BootAsync(RingOptions options, CancellationToken cancellationToken)
	{
		ValidateOptions(options);
		var config = BuildConfiguration(options);
		Global.Init(config);
		//var connection = ConnectionFactory.Create(options.Provider, 0, config);
		//await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
		var pool = new ConnectionPool(
			1,
			options.MinPoolSize,
			options.MaxPoolSize,
			resizeCount: 0,
			options.ConnectionString);
		//await pool.InitAsync(connection, cancellationToken).ConfigureAwait(false);
		//Global.SetConnectionPool(pool);
	}

	// ── Helpers ────────────────────────────────────────────────────────────────

	private static void EnsureNotStarted()
	{
		if (Volatile.Read(ref _state) == 2)
			throw new InvalidOperationException(
				"Ring engine is already started. Call Initialize.Configure() only once per process.");
	}

	private static void ValidateOptions(RingOptions options)
	{
		if (string.IsNullOrWhiteSpace(options.ConnectionString))
			throw new ArgumentException("A connection string is required.", nameof(options));
		if (options.MinPoolSize < 1)
			throw new ArgumentOutOfRangeException(nameof(options), "MinPoolSize must be >= 1.");
		if (options.MaxPoolSize < options.MinPoolSize)
			throw new ArgumentOutOfRangeException(nameof(options), "MaxPoolSize must be >= MinPoolSize.");
		if (options.MaxSchemas < 1)
			throw new ArgumentOutOfRangeException(nameof(options), "MaxSchemas must be >= 1.");
	}

	private static IConfiguration BuildConfiguration(RingOptions options) =>
		throw new NotImplementedException();

}
