using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Thin
{
    public class UpdateOneCommand<TDocument>
    {
        private readonly IMongoCollection<TDocument> _collection;
        private FilterBuilder<TDocument> _filter = new FilterBuilder<TDocument>();
        private UpdateBuilder<TDocument> _update = new UpdateBuilder<TDocument>();
        private UpdateOptionsBuilder<TDocument> _options = new UpdateOptionsBuilder<TDocument>();

        public UpdateOneCommand(IMongoCollection<TDocument> collection)
        {
            _collection = collection;
        }

        public UpdateOneCommand<TDocument> Filter(Action<FilterBuilder<TDocument>> withBuilder)
        {
            withBuilder(_filter);
            return this;
        }

        public UpdateOneCommand<TDocument> Update(Action<UpdateBuilder<TDocument>> withBuilder)
        {
            withBuilder(_update);
            return this;
        }

        public UpdateOneCommand<TDocument> Options(Action<UpdateOptionsBuilder<TDocument>> withBuilder)
        {
            withBuilder(_options);
            return this;
        }

        public Task<UpdateResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return _collection.UpdateOneAsync(_filter, _update, _options, cancellationToken);
        }

        public Task<UpdateResult> ExecuteAsync(IClientSessionHandle clientSession, CancellationToken cancellationToken = default)
        {
            return _collection.UpdateOneAsync(clientSession, _filter, _update, _options, cancellationToken);
        }
    }
}
