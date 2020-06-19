using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Domain
{
    public class PrefilledUniqueRandomRangeCreator : IUniqueRandomRangeCreator
    {
        private readonly IUniqueRandomRangeCreator _rangeCreator;
        private readonly UniqueRandomRange[][] _rangesByRangeMax;
        private readonly Random _random;

        private const int LowerRangeMax = 2;
        private const int UpperRangeMax = 20;

        public PrefilledUniqueRandomRangeCreator(IUniqueRandomRangeCreator rangeCreator)
        {
            _rangeCreator = rangeCreator;
            _random = new Random();

            _rangesByRangeMax = new UniqueRandomRange[UpperRangeMax][];
            foreach (var group in GenerateUniqueRandomRangesToCache(LowerRangeMax, UpperRangeMax).GroupBy(g => g.RangeMax))
            {
                _rangesByRangeMax[group.Key] = group.ToArray();
            }
        }

        public IReadOnlyList<int> CreateArrayWithAllNumbersFromRange(int rangeMax)
        {
            if (rangeMax < LowerRangeMax) throw new Exception($"RangeMax should be at least {LowerRangeMax}");
            if (rangeMax > UpperRangeMax) throw new Exception($"RangeMax should be at most {UpperRangeMax}");

            var ranges = _rangesByRangeMax[rangeMax];
            var rangeIndex = _random.Next(ranges.Length);
            return ranges[rangeIndex].Values;
        }
        
        private IEnumerable<UniqueRandomRange> GenerateUniqueRandomRangesToCache(int rangeMinToGenerate, int rangeMaxToGenerate)
        {
            for (var rangeMax=rangeMinToGenerate; rangeMax<rangeMaxToGenerate; rangeMax++)
            {
                for (var trial=0; trial<rangeMax*2; trial++)
                {
                    var values = _rangeCreator.CreateArrayWithAllNumbersFromRange(rangeMax);
                    yield return new UniqueRandomRange(rangeMax, values);
                }
            }
        }
    }
}
