using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace MongoDB.Thin
{
    public class FilterBuilder<TDocument>
    {
        private FilterDefinition<TDocument> _filter = Builders<TDocument>.Filter.Empty;

        /// <summary>
        /// Specify the matching criteria with a lambda expression
        /// </summary>
        /// <param name="expression">A lambda expression to select the documents to update</param>
        public FilterBuilder<TDocument> Match(Expression<Func<TDocument, bool>> expression)
        {
            return Match(f => f.Where(expression));
        }

        /// <summary>
        /// Specify the matching criteria with a filter expression
        /// </summary>
        /// <param name="filter">f => f.Eq(x => x.Prop, Value) &amp; f.Gt(x => x.Prop, Value)</param>
        public FilterBuilder<TDocument> Match(Func<FilterDefinitionBuilder<TDocument>, FilterDefinition<TDocument>> filter)
        {
            _filter &= filter(Builders<TDocument>.Filter);
            return this;
        }

        /// <summary>
        /// Specify the matching criteria with a JSON string
        /// </summary>
        /// <param name="jsonString">{ Title : 'The Power Of Now' }</param>
        public FilterBuilder<TDocument> Match(string json)
        {
            _filter &= json;
            return this;
        }

        public bool IsEmpty => _filter == Builders<TDocument>.Filter.Empty;

        public FilterDefinition<TDocument> Build() => _filter;

        public static implicit operator FilterDefinition<TDocument>(FilterBuilder<TDocument> builder)
        {
            return builder.Build();
        }
    }
}
