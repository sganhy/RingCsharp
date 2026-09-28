using Ring.Schema.Enums;
using System.Text;

namespace Ring.Util.Builders;

internal abstract class BaseTclBuilder : BaseSqlBuilder, ITclBuilder
{
	protected BaseTclBuilder(DatabaseProvider provider, Encoding clientEncoding) : base(provider, clientEncoding) {}

	public ReadOnlySpan<byte> Commit(Encoding encoding)
	{
		throw new NotImplementedException();
	}

	public ReadOnlySpan<byte> Rollback(Encoding encoding)
	{
		throw new NotImplementedException();
	}

	public ReadOnlySpan<byte> StartTransaction(Encoding encoding)
	{
		throw new NotImplementedException();
	}
}
