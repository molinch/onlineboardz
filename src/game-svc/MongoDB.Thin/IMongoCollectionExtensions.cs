using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDB.Thin
{
    public static class IMongoCollectionExtensions
    {
        public static async Task<TDocument?> FindOneAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> predicate)
            where TDocument: class
        {
            var cursor = await collection.FindAsync(predicate);
            return await cursor.SingleOrDefaultAsync();
        }

        public static FindOneAndUpdateCommand<TDocument, TDocument> FindOneAndUpdate<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return new FindOneAndUpdateCommand<TDocument, TDocument>(collection);
        }

        public static UpdateOneCommand<TDocument> UpdateOne<TDocument>(this IMongoCollection<TDocument> collection)
        {
            return new UpdateOneCommand<TDocument>(collection);
        }
    }
}
