using Ring.Schema.Enums;
using System.Runtime.CompilerServices;


namespace Ring;

public static class Initialize
{
    public static void Start(Type connectionType,string connectionString)
    {
        Global.Start("information_schema", connectionType, connectionString, 
            DatabaseProvider.PostgreSql, 3);

    }
}
