using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Thin
{
    public class FindOneAndUpdateCommand<TDocument, TProjection>
    {
        private readonly IMongoCollection<TDocument> _collection;
        private FilterBuilder<TDocument> _filter = new FilterBuilder<TDocument>();
        private UpdateBuilder<TDocument> _update = new UpdateBuilder<TDocument>();
        private FindOneAndUpdateOptionsBuilder<TDocument, TProjection> _options = new FindOneAndUpdateOptionsBuilder<TDocument, TProjection>();

        public FindOneAndUpdateCommand(IMongoCollection<TDocument> collection)
        {
            _collection = collection;
        }

        public FindOneAndUpdateCommand<TDocument, TProjection> Filter(Action<FilterBuilder<TDocument>> withBuilder)
        {
            withBuilder(_filter);
            return this;
        }

        public FindOneAndUpdateCommand<TDocument, TProjection> Update(Action<UpdateBuilder<TDocument>> withBuilder)
        {
            withBuilder(_update);
            return this;
        }

        public FindOneAndUpdateCommand<TDocument, TProjection> Options(Action<FindOneAndUpdateOptionsBuilder<TDocument, TProjection>> withBuilder)
        {
            withBuilder(_options);
            return this;
        }

        public Task<TProjection> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndUpdateAsync<TProjection>(_filter, _update, _options, cancellationToken);
        }

        public Task<TProjection> ExecuteAsync(IClientSessionHandle clientSession, CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndUpdateAsync<TProjection>(clientSession, _filter, _update, _options, cancellationToken);
        }
    }
}
