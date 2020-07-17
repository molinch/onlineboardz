using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoDB.Thin
{
    public class UpdateBuilder<TDocument>
    {
        private readonly List<UpdateDefinition<TDocument>> _updates = new List<UpdateDefinition<TDocument>>();

        /// <summary>
        /// Specify the property and it's value to modify (use multiple times if needed)
        /// </summary>
        /// <param name="property">x => x.Property</param>
        /// <param name="value">The value to set on the property</param>
        public UpdateBuilder<TDocument> Modify<TProp>(Expression<Func<TDocument, TProp>> property, TProp value)
        {
            _updates.Add(Builders<TDocument>.Update.Set(property, value));
            return this;
        }

        /// <summary>
        /// Specify the update definition builder operation to modify the documents (use multiple times if needed)
        /// </summary>
        /// <param name="operation">b => b.Inc(x => x.PropName, Value)</param>
        public UpdateBuilder<TDocument> Modify(Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> operation)
        {
            _updates.Add(operation(Builders<TDocument>.Update));
            return this;
        }

        /// <summary>
        /// Specify an update (json string) to modify the documents (use multiple times if needed)
        /// </summary>
        /// <param name="update">{ $set: { 'RootProp.$[x].SubProp' : 321 } }</param>
        public UpdateBuilder<TDocument> Modify(string update)
        {
            _updates.Add(update);
            return this;
        }

        public UpdateDefinition<TDocument> Build() => Builders<TDocument>.Update.Combine(_updates);

        public static implicit operator UpdateDefinition<TDocument>(UpdateBuilder<TDocument> builder)
        {
            return builder.Build();
        }
    }
}
