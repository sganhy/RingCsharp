using System.Text;

namespace Ring.Util.Builders;

/// <summary>
/// Transact Control Language: (TCL) Builder interface for building TCL statements.
/// </summary>
internal interface ITclBuilder : ISqlBuilder
{
	ReadOnlySpan<byte> Commit(Encoding encoding);
	ReadOnlySpan<byte> StartTransaction(Encoding encoding);
	ReadOnlySpan<byte> Rollback(Encoding encoding);
}
