using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace MongoDB.Thin
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services, string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            services.AddSingleton<IMongoClient>(client);
            services.AddSingleton(client.GetDatabase(databaseName));
            return services;
        }
    }
}
