using Ring.Schema.Extensions;
using Ring.Schema.Models;

namespace Ring;

public static class Initialize
{
    public static void Start(Type connectionType,string connectionString)
    {
        var connPool = new ConnectionPool(0, 3, 10, connectionString, connectionType);
        connPool.Init();

        var con1 = connPool.Get();
        var con2 = connPool.Get();
        var con3 = connPool.Get();

        connPool.Put(con3);
        connPool.Put(con2);
        connPool.Put(con1);

        int oi = 0;
        ++oi;

    }
}
