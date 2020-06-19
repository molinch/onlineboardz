using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace ApiTests.Persistence
{
    public abstract class RepositoryTests : IDisposable
    {
        private static readonly Random _random = new Random();
        private readonly IConfigurationRoot _configuration;
        protected readonly string _dbName;
        protected readonly MongoClientSettings _mongoClientSettings;

        public RepositoryTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(GameRepositoryTests).Assembly)
                .AddJsonFile("appsettings.json")
                .Build();

            // The idea is that each test run gets his own fresh database (dropped during Dispose)
            // This way there is no state shared between the tests
            // Note: seems like Mongo has an inmemory feature that we could leverage too
            _dbName = "GameDbTest-" + _random.Next();
            _mongoClientSettings = MongoClientSettings.FromConnectionString(_configuration.GetValue<string>("MongoConnectionString"));
        }

        public void Dispose()
        {
            var client = new MongoClient(_mongoClientSettings);
            client.DropDatabase(_dbName);
        }
    }
}
