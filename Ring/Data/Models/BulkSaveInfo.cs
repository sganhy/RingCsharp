using Ring.Schema.Enums;
using Ring.Schema;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data.Models;

internal sealed class BulkSaveInfo
{
	private static readonly Database DefaultSchema = Meta.GetEmptySchema(new Meta(-1, (byte)EntityType.Schema, 0, 0, 0L, string.Empty, null, null, true),
					DatabaseProvider.Undefined);
	internal SpanList<SaveQuery> Queries;
	internal Database Schema;
	internal int IdCount;

	public BulkSaveInfo(int initBucketSize, Database? schema=null)
	{
		Queries = new SpanList<SaveQuery>(initBucketSize);
		Schema = schema ?? DefaultSchema;
		IdCount = 0;
	}
}
