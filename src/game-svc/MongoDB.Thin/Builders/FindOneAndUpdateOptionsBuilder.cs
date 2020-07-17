using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoDB.Thin
{
    public class FindOneAndUpdateOptionsBuilder<TDocument, TProjection>
    {
        private readonly List<ArrayFilterDefinition<TDocument>> _arrayFilters = new List<ArrayFilterDefinition<TDocument>>();
        private readonly FindOneAndUpdateOptions<TDocument, TProjection> _options = new FindOneAndUpdateOptions<TDocument, TProjection>()
        {
            ReturnDocument = ReturnDocument.After
        };

        /// <summary>
        /// Specify an array filter to target nested documents for updates (use multiple times if needed).
        /// </summary>
        /// <param name="filter">{ 'x.SubProp': { $gte: 123 } }</param>
        public FindOneAndUpdateOptionsBuilder<TDocument, TProjection> WithArrayFilter(string filter)
        {
            _arrayFilters.Add(filter);
            return this;
        }

        /// <summary>
        /// Specify how to project the results using a lambda expression
        /// </summary>
        /// <param name="expression">x => new Test { PropName = x.Prop }</param>
        public FindOneAndUpdateOptionsBuilder<TDocument, TProjection> Project(Expression<Func<TDocument, TProjection>> expression)
        {
            return Project(p => p.Expression(expression));
        }

        /// <summary>
        /// Specify how to project the results using a projection expression
        /// </summary>
        /// <param name="projection">p => p.Include("Prop1").Exclude("Prop2")</param>
        public FindOneAndUpdateOptionsBuilder<TDocument, TProjection> Project(Func<ProjectionDefinitionBuilder<TDocument>, ProjectionDefinition<TDocument, TProjection>> projection)
        {
            _options.Projection = projection(Builders<TDocument>.Projection);
            return this;
        }

        public FindOneAndUpdateOptions<TDocument, TProjection> Build()
        {
            if (_arrayFilters.Count > 0)
            {
                _options.ArrayFilters = _arrayFilters;
            }

            return _options;
        }

        public static implicit operator FindOneAndUpdateOptions<TDocument, TProjection>(FindOneAndUpdateOptionsBuilder<TDocument, TProjection> builder)
        {
            return builder.Build();
        }
    }
}
