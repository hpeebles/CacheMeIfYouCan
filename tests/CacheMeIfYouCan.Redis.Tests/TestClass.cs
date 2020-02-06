using System;
using System.Runtime.Serialization;

namespace CacheMeIfYouCan.Redis.Tests
{
    [DataContract]
    public class TestClass
    {
        public TestClass() { }
        
        public TestClass(int value)
        {
            IntValue = value;
            StringValue = value.ToString();
        }
        
        [DataMember(Order = 1)]
        public int IntValue { get; set; }
        
        [DataMember(Order = 2)]
        public string StringValue { get; set; }

        public override string ToString() => IntValue + "_" + StringValue;

        public static TestClass Parse(string str)
        {
            var parts = str.Split("_");

            return new TestClass
            {
                IntValue = Int32.Parse(parts[0]),
                StringValue = parts[1]
            };
        }
    }
}