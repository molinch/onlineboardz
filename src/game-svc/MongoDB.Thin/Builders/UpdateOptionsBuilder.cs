using MongoDB.Driver;
using System.Collections.Generic;

namespace MongoDB.Thin
{
    public class UpdateOptionsBuilder<TDocument>
    {
        private readonly List<ArrayFilterDefinition<TDocument>> _arrayFilters = new List<ArrayFilterDefinition<TDocument>>();
        private readonly UpdateOptions _options = new UpdateOptions();

        /// <summary>
        /// Specify an array filter to target nested documents for updates (use multiple times if needed).
        /// </summary>
        /// <param name="filter">{ 'x.SubProp': { $gte: 123 } }</param>
        public UpdateOptionsBuilder<TDocument> WithArrayFilter(string filter)
        {
            _arrayFilters.Add(filter);
            return this;
        }

        public UpdateOptions Build()
        {
            if (_arrayFilters.Count > 0)
            {
                _options.ArrayFilters = _arrayFilters;
            }

            return _options;
        }

        public static implicit operator UpdateOptions(UpdateOptionsBuilder<TDocument> builder)
        {
            return builder.Build();
        }
    }
}
