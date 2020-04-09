using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Tests.Generators
{
    public static class EnumAndBoolGenerator<TEnum>
    {
        public static IEnumerable<object[]> Generate()
        {
            foreach (var enumValue in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                yield return new object[] { enumValue, true };
                yield return new object[] { enumValue, false };
            }
        }
    }
}