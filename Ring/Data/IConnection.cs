using Ring.Data.Models;
using System.Text;

namespace Ring.Data;

/// <summary>
/// Represents a database connection instance and provides low-level SQL execution methods.
/// </summary>
public interface IConnection : IDisposable
{
	/// <summary>
	/// Determines whether the connection to the backend database server is currently active and responsive.
	/// </summary>
	/// <returns><c>true</c> if the connection is alive; otherwise, <c>false</c>.</returns>
	bool IsConnectionAlive();

	/// <summary>
	/// Gets a value indicating whether prepared statements have been declared for the current session.
	/// </summary>
	bool HasDeclaredStmts { get; }

	/// <summary>
	/// Gets the unique identifier for the underlying database provider.
	/// </summary>
	int ProviderId { get; }

	/// <summary>
	/// Gets the process ID (PID) of the backend server process associated with this connection.
	/// </summary>
	int BackendPid { get; }

	/// <summary>
	/// Gets the secret key associated with the backend process, typically used for cancellation requests.
	/// </summary>
	int BackendSecret { get; }

	/// <summary>
	/// Gets the unique internal identifier assigned to this connection instance.
	/// </summary>
	long Id { get; }

	/// <summary>
	/// Gets the timestamp indicating when this connection instance was created.
	/// </summary>
	DateTime CreationTime { get; }

	/// <summary>
	/// Gets the timestamp of the last successful interaction over this connection, or <c>null</c> if never opened.
	/// </summary>
	DateTime? LastConnectionTime { get; }

	/// <summary>
	/// Gets the current state of the database connection.
	/// </summary>
	ConnectionState State { get; }

	/// <summary>
	/// Gets the character encoding used for text transmission between the client and server.
	/// </summary>
	Encoding ClientEncoding { get; }

	/// <summary>
	/// Opens the database connection synchronously.
	/// </summary>
	void Open();

	/// <summary>
	/// Opens the database connection asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
	/// <returns>A task representing the asynchronous open operation.</returns>
	ValueTask OpenAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Closes the database connection synchronously.
	/// </summary>
	void Close();

	/// <summary>
	/// Closes the database connection asynchronously.
	/// </summary>
	/// <returns>A task representing the asynchronous close operation.</returns>
	ValueTask CloseAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Creates a new child or pooled instance of the connection with specific buffer sizing.
	/// </summary>
	/// <param name="id">The unique identifier to assign to the new connection instance.</param>
	/// <param name="sqlSendBufferSize">The buffer size (in bytes) allocated for sending SQL payloads.</param>
	/// <returns>A new <see cref="IConnection"/> instance configured with the specified parameters.</returns>
	IConnection CreateInstance(int id, int sqlSendBufferSize);

	/// <summary>
	/// Executes a raw SQL command buffer without expecting a returned data set.
	/// </summary>
	/// <param name="sql">The raw UTF-8 encoded SQL byte span to execute.</param>
	/// <returns>An <see cref="OperationalError"/> if execution failed; otherwise, <c>null</c>.</returns>
	OperationalError? Execute(ReadOnlySpan<byte> sql);

	/// <summary>
	/// Executes a data retrieval query and returns the matching string field values.
	/// </summary>
	/// <param name="query">The query specification parameters passed by readonly reference.</param>
	/// <param name="sql">The raw UTF-8 encoded SQL byte span to execute.</param>
	/// <returns>An array of nullable strings representing the query result set columns.</returns>
	string?[] Execute(in RetrieveQuery query, ReadOnlySpan<byte> sql);

	/// <summary>
	/// Executes a schema or structural modification query synchronously.
	/// </summary>
	/// <param name="query">The alter query metadata passed by readonly reference to prevent defensive copy penalties.</param>
	/// <param name="sql">The raw UTF-8 encoded SQL byte span to execute.</param>
	/// <returns>An <see cref="OperationalError"/> if execution failed; otherwise, <c>null</c>.</returns>
	OperationalError? Execute(in AlterQuery query, ReadOnlySpan<byte> sql); // AlterQuery: No defensive-copy penalty — the JIT knows no member access can mutate it.

	/// <summary>
	/// Executes a schema or structural modification query asynchronously.
	/// </summary>
	/// <param name="query">The alter query metadata.</param>
	/// <param name="sql">The raw UTF-8 encoded SQL byte memory buffer to execute.</param>
	/// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
	/// <returns>A value task returning an <see cref="OperationalError"/> if execution failed; otherwise, <c>null</c>.</returns>
	ValueTask<OperationalError?> ExecuteAsync(AlterQuery query, ReadOnlyMemory<byte> sql, CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes a persistence or insert/update query synchronously.
	/// </summary>
	/// <param name="query">The save query specification passed by readonly reference.</param>
	/// <param name="sql">The raw UTF-8 encoded SQL byte span to execute.</param>
	/// <returns>An <see cref="OperationalError"/> if execution failed; otherwise, <c>null</c>.</returns>
	OperationalError? Execute(in SaveQuery query, ReadOnlySpan<byte> sql);
}