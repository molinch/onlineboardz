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
            _dbName = "GameDbTest-" + Guid.NewGuid();
            _mongoClientSettings = MongoClientSettings.FromConnectionString(_configuration.GetValue<string>("MongoConnectionString"));
            _mongoClientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    var command = e.Command.ToJson();
                    Console.WriteLine(command + Environment.NewLine);
                });
            };
        }

        public void Dispose()
        {
            var client = new MongoClient(_mongoClientSettings);
            client.DropDatabase(_dbName);
        }
    }
}
