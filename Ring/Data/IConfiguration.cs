namespace Ring.Data
{
    public interface IConfiguration
    {
        /// <summary>
        /// Could be 
        /// </summary>
        string Driver { get; }
        string ConnectionString { get; }
    }
}
