using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public class Configuration : IConfiguration
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="connectionString"></param>
        public Configuration(string driver, string  connectionString)
        {
            Driver = driver;
            ConnectionString = connectionString;
        }

        public string Driver { get; }
        public string ConnectionString { get; }
    }
}
