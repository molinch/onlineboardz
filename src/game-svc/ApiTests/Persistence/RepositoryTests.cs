using Api;
using ConfigurationSubstitution;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System;

namespace ApiTests.Persistence
{
    public abstract class RepositoryTests : IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        private readonly string _dbName;
        private readonly MongoClientSettings _mongoClientSettings;
        private readonly MongoClient _client;
        protected readonly IMongoDatabase _database;

        public RepositoryTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(GameRepositoryTests).Assembly)
                .AddJsonFile("appsettings.Development.json") // to do fix connection string with pwd
                .EnableSubstitutions()
                .Build();

            // The idea is that each test run gets his own fresh database (dropped during Dispose)
            // This way there is no state shared between the tests
            // Note: seems like Mongo has an inmemory feature that we could leverage too
            _dbName = "GameDbTest-" + Guid.NewGuid();
            string connectionString = _configuration["MongoConnectionString"];
            _mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);
            _mongoClientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    var command = e.Command.ToJson();
                    Console.WriteLine(command + Environment.NewLine);
                });
            };

            _client = new MongoClient(_mongoClientSettings);
            _database = _client.GetDatabase(_dbName);
        }

        public void Dispose()
        {
            _client.DropDatabase(_dbName);
        }
    }
}
