using NpgsqlTypes;
using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.PostgreSQL.Extensions;

internal static class FieldTypeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static NpgsqlDbType ToNpgsqlDbType(this FieldType fieldType)
    {
        // dateTime?? 
        switch (fieldType)
        {
            case FieldType.Long: return NpgsqlDbType.Bigint;
            case FieldType.Int: return NpgsqlDbType.Integer;
            case FieldType.Short:
            case FieldType.Byte: return NpgsqlDbType.Smallint;
            case FieldType.Float: return NpgsqlDbType.Real;
            case FieldType.Double: return NpgsqlDbType.Double;
            case FieldType.String: return NpgsqlDbType.Varchar;
            case FieldType.LongString: return NpgsqlDbType.Text;
            case FieldType.Boolean: return NpgsqlDbType.Boolean;
            case FieldType.ShortDateTime: return NpgsqlDbType.Varchar;
            case FieldType.DateTime: return NpgsqlDbType.Varchar;
            case FieldType.LongDateTime: return NpgsqlDbType.TimestampTz;
            case FieldType.ByteArray: return NpgsqlDbType.Bytea;
        }
        throw new ArgumentException($"Field type '{fieldType}' is not supported.");
    }
}
