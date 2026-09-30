using Ring.Util.Models;
using System.Text;

namespace Ring.Util.Builders;

/// <summary>
/// Transact Control Language: (TCL) Builder interface for building TCL statements.
/// </summary>
internal interface ITclBuilder : ISqlBuilder
{
	SqlEntry Commit { get; }
	SqlEntry StartTransaction { get; }
	SqlEntry Rollback { get; }
}
