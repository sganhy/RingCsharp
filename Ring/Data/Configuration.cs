namespace Ring.Data;

public sealed class Configuration : IConfiguration
{
    public string? ConnectionString { get; set; }
    public int MinConnectionPoolSize { get; set; } = 1;
    public int MaxConnectionPoolSize { get; set; } = 1;
    public string DefaultSchema { get; set; } = string.Empty;
    public string? DefaultIndexStorage { get; set; }
    public string? DefaultTableStorage { get; set; }
    public int MaxNumberOfSchema { get; set; }
}
