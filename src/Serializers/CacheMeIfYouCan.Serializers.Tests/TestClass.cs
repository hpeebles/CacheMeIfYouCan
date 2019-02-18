using System.Runtime.Serialization;

namespace CacheMeIfYouCan.Serializers.Tests
{
    [DataContract]
    public class TestClass
    {
        [DataMember(Order = 1)]
        public int IntValue { get; set; }
        
        [DataMember(Order=  2)]
        public string StringValue { get; set; }

        public override string ToString()
        {
            return $"{IntValue}_{StringValue}";
        }
    }
}