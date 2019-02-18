using System.Runtime.Serialization;

namespace CacheMeIfYouCan.Serializers.Protobuf.Tests
{
    [DataContract]
    public class TestClass
    {
        [DataMember(Order = 1)]
        public int IntValue { get; set; }
        
        [DataMember(Order=  2)]
        public string StringValue { get; set; }
    }
}