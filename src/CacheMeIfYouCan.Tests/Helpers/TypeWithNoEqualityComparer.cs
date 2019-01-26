namespace CacheMeIfYouCan.Tests.Helpers
{
    public class TypeWithNoEqualityComparer
    {
        public TypeWithNoEqualityComparer(int value)
        {
            Value = value;
        }
        
        public int Value { get; }
    }
}