using System.Collections.Generic;

namespace Api.Domain
{
    public class UniqueRandomRange
    {
        public UniqueRandomRange(int rangeMax, IReadOnlyList<int> values)
        {
            RangeMax = rangeMax;
            Values = values;
        }

        public int RangeMax { get; }
        public IReadOnlyList<int> Values { get; }
    }
}
