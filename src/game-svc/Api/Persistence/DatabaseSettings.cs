using Microsoft.Extensions.Configuration;

namespace Api.Persistence
{
    public class DatabaseSettings
    {
        private readonly IConfiguration _configuration;

        public DatabaseSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ConnectionString => _configuration.GetValue<string>("MongoConnectionString");
        public string GameDatabaseName => _configuration.GetValue<string>("GameDatabaseName");
    }
}
