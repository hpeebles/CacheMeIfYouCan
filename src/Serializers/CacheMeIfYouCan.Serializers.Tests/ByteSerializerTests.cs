using System;
using CacheMeIfYouCan.Serializers.Gzip;
using CacheMeIfYouCan.Serializers.Protobuf;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Serializers.Tests
{
    public class ByteSerializerTests
    {
        [Theory]
        [InlineData("protobuf", false)]
        [InlineData("protobuf", true)]
        public void SerializeTests(string name, bool useGzip)
        {
            var serializer = GetSerializer(name);

            if (useGzip)
                serializer = serializer.WithGzipCompression();
            
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

            serialized1.Should().BeEquivalentTo(serialized2);
        }
        
        [Theory]
        [InlineData("protobuf", false)]
        [InlineData("protobuf", true)]
        public void DeserializeTests(string name, bool useGzip)
        {
            var serializer = GetSerializer(name);

            if (useGzip)
                serializer = serializer.WithGzipCompression();
            
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

        private static IByteSerializer GetSerializer(string name)
        {
            switch (name)
            {
                case "protobuf":
                    return new ProtobufSerializer();
                    
                default:
                    throw new Exception($"No byte serializer found matching name '{name}'");
            }
        }
    }
}