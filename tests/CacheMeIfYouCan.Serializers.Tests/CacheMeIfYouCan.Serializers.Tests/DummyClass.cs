using System.Runtime.Serialization;

namespace CacheMeIfYouCan.Serializers.Tests
{
    [DataContract]
    public class DummyClass
    {
        [DataMember(Order = 1)]
        public int IntValue { get; set; }
        
        [DataMember(Order = 2)]
        public string StringValue { get; set; }

        protected bool Equals(DummyClass other)
        {
            return IntValue == other.IntValue && StringValue == other.StringValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(DummyClass)) return false;
            return Equals((DummyClass) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IntValue * 397) ^ StringValue.GetHashCode();
            }
        }
    }
}