using System;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Keys
{
    public class KeyComparerResolver
    {
        [Theory]
        [InlineData("int", true)]
        [InlineData("DateTime", true)]
        [InlineData("WithOverrides", true)]
        [InlineData("WithoutOverrides", false)]
        public void ReturnsCorrectResolver(string typeName, bool expectGenericComparer)
        {
            switch (typeName)
            {
                case "int":
                    CheckType<int>(expectGenericComparer);
                    break;

                case "DateTime":
                    CheckType<DateTime>(expectGenericComparer);
                    break;

                case "WithOverrides":
                    CheckType<WithOverrides>(expectGenericComparer);
                    break;
                
                case "WithoutOverrides":
                    CheckType<WithoutOverrides>(expectGenericComparer);
                    break;
                
                default:
                    throw new Exception($"Unexpected typeName '{typeName}'");
            }
        }

        private static void CheckType<T>(bool expectGenericComparer)
        {
            var comparer = Internal.KeyComparerResolver.Get<T>();

            var expectedType = expectGenericComparer
                ? typeof(GenericKeyComparer<T>)
                : typeof(StringKeyComparer<T>);

            comparer.Should().BeOfType(expectedType);
        }

        private class WithOverrides
        {
            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                throw new NotImplementedException();
            }
        }

        public class WithoutOverrides
        {
        }
    }
}