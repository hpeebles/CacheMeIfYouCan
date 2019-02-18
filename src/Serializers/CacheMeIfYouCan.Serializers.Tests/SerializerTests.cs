using System;
using CacheMeIfYouCan.Serializers.Json.Newtonsoft;
using CacheMeIfYouCan.Serializers.Protobuf;
using CacheMeIfYouCan.Serializers.ToString;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Serializers.Tests
{
    public class SerializerTests
    {
        [Theory]
        [InlineData("newtonsoft")]
        [InlineData("protobuf")]
        [InlineData("tostring")]
        public void SerializeTests(string name)
        {
            var serializer = GetSerializer(name);
            
            var intValue = (int)DateTime.UtcNow.Ticks;
            var stringValue = Guid.NewGuid().ToString();

            var obj1 = new TestClass
            {
                IntValue = intValue,
                StringValue = stringValue
            };
            
            var obj2 = new TestClass
            {
                IntValue = intValue,
                StringValue = stringValue
            };

            var serialized1 = serializer.Serialize(obj1);
            var serialized2 = serializer.Serialize(obj2);

            serialized1.Should().Be(serialized2);
        }
        
        [Theory]
        [InlineData("newtonsoft")]
        [InlineData("protobuf")]
        [InlineData("tostring")]
        public void DeserializeTests(string name)
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
            
            if (name == "tostring")
            {
                Func<TestClass> deserialize = () => serializer.Deserialize<TestClass>(serialized);
                deserialize.Should().ThrowExactly<NotImplementedException>();
            }
            else
            {
                var deserialized = serializer.Deserialize<TestClass>(serialized);

                deserialized.IntValue.Should().Be(intValue);
                deserialized.StringValue.Should().Be(stringValue);
            }
        }

        private static ISerializer GetSerializer(string name)
        {
            switch (name)
            {
                case "newtonsoft":
                    return new JsonSerializer();
                
                case "protobuf":
                    return new ProtobufSerializer();
                    
                case "tostring":
                    return new ToStringSerializer();
                
                default:
                    throw new Exception($"No serializer found match name '{name}'");
            }
        }
    }
}