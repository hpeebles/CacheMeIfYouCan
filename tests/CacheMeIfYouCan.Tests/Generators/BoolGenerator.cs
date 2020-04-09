using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Tests.Generators
{
    public static class BoolGenerator
    {
        public static IEnumerable<object[]> GetAllCombinations(int count)
        {
            for (var i = 0; i < Math.Pow(2, count); i++)
            {
                var values = new object[count];
                for (var j = 0; j < count; j++)
                    values[j] = (i & (1 << j)) != 0;

                yield return values;
            }
        }
    }
}