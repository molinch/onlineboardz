using System.Collections.Generic;

namespace Api.Domain
{
    public interface IUniqueRandomRangeCreator
    {
        IReadOnlyList<int> CreateArrayWithAllNumbersFromRange(int rangeMax);
    }
}