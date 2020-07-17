using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoDB.Thin
{
    public static class IMongoIndexManagerExtensions
    {
        public static IMongoIndexManager<TDocument> Add<TDocument>(this IMongoIndexManager<TDocument> indexManager,
            string path)
        {
            indexManager.CreateOne(new CreateIndexModel<TDocument>(
                new IndexKeysDefinitionBuilder<TDocument>()
                    .Ascending(new StringFieldDefinition<TDocument>(path))
            ));

            return indexManager;
        }

        public static IMongoIndexManager<TDocument> Add<TDocument>(this IMongoIndexManager<TDocument> indexManager,
            Expression<Func<TDocument, object>> targetField)
        {
            return indexManager.Add(Path.From(targetField));
        }

        public static IMongoIndexManager<TDocument> Add<TDocument, TArrayDocument>(this IMongoIndexManager<TDocument> indexManager,
            Expression<Func<TDocument, IEnumerable<TArrayDocument>>> targetArray, Expression<Func<TArrayDocument, object>> targetField)
        {
            return indexManager.Add(Path.From(targetArray, targetField));
        }
    }
}
