using System;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Serializers.Protobuf.Tests
{
    public class ProtobufSerializerTests
    {
        private readonly ISerializer _serializer = new ProtobufSerializer();
        
        [Fact]
        public void Test()
        {
            var intValue = (int)DateTime.UtcNow.Ticks;
            var stringValue = Guid.NewGuid().ToString();

            var obj = new TestClass
            {
                IntValue = intValue,
                StringValue = stringValue
            };

            var serialized = _serializer.Serialize(obj);

            var deserialized = _serializer.Deserialize<TestClass>(serialized);

            deserialized.IntValue.Should().Be(intValue);
            deserialized.StringValue.Should().Be(stringValue);
        }
    }
}