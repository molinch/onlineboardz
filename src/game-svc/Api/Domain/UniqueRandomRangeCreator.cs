using System;
using System.Collections.Generic;

namespace Api.Domain
{
    public class UniqueRandomRangeCreator : IUniqueRandomRangeCreator
    {
        private readonly Random _random;

        public UniqueRandomRangeCreator()
        {
            _random = new Random();
        }

        public IReadOnlyList<int> CreateArrayWithAllNumbersFromRange(int rangeMax)
        {
            var uniqueSet = new HashSet<int>();
            int[] playerOrders = new int[rangeMax];
            for (int i = 0; i < rangeMax; i++)
            {
                int value;
                do
                {
                    value = _random.Next(0, rangeMax); // first value is inclusive, second is exclusive (that's why we don't put a -1)
                } while (uniqueSet.Contains(value));
                uniqueSet.Add(value);
                playerOrders[i] = value;
            }
            return playerOrders;
        }
    }
}
