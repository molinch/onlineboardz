using System;
using System.Collections.Generic;

namespace Api.Commands
{
    public static class UniqueRandomRange
    {
        public static int[] CreateArrayWithAllNumbersFromRange(int rangeMax)
        {
            var random = new Random();
            var uniqueSet = new HashSet<int>();
            int[] playerOrders = new int[rangeMax];
            for (int i = 0; i < rangeMax; i++)
            {
                int value;
                do
                {
                    value = random.Next(0, rangeMax - 1);
                } while (uniqueSet.Contains(value));
                uniqueSet.Add(value);
                playerOrders[i] = value;
            }
            return playerOrders;
        }
    }
}
