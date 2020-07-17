using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MongoDB.Thin
{
    public static class IMongoDatabaseExtensions
    {
        public static IMongoCollection<TDocument> Collection<TDocument>(this IMongoDatabase database)
        {
            return database.GetCollection<TDocument>(typeof(TDocument).Name);
        }
    }
}
