using System;
using CacheMeIfYouCan.Serializers.Json.Newtonsoft;
using CacheMeIfYouCan.Serializers.Protobuf;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Serializers.Tests
{
    public class SerializerTests
    {
        [Theory]
        [InlineData("newtonsoft")]
        [InlineData("protobuf")]
        public void Test(string name)
        {
            var serializer = GetSerializer(name);
            
            var intValue = (int)DateTime.UtcNow.Ticks;
            var stringValue = Guid.NewGuid().ToString();

            var obj = new TestClass
            {
                IntValue = intValue,
                StringValue = stringValue
            };

            var serialized = serializer.Serialize(obj);

            var deserialized = serializer.Deserialize<TestClass>(serialized);

            deserialized.IntValue.Should().Be(intValue);
            deserialized.StringValue.Should().Be(stringValue);
        }

        private ISerializer GetSerializer(string name)
        {
            switch (name)
            {
                case "newtonsoft":
                    return new JsonSerializer();
                
                case "protobuf":
                    return new ProtobufSerializer();
                    
                default:
                    throw new Exception($"No serializer found match name '{name}'");
            }
        }
    }
}