using Ring.Data;

namespace Ring.PostgreSQL;

public sealed class Configuration : IConfiguration
{
    public string? ConnectionString { get ; set; }


}
