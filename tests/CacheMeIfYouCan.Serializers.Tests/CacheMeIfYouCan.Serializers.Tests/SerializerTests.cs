using System;
using System.IO;
using CacheMeIfYouCan.Serializers.Json;
using CacheMeIfYouCan.Serializers.ProtoBuf;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Serializers.Tests
{
    public class SerializerTests
    {
        [Theory]
        [InlineData("json", true)]
        [InlineData("json", false)]
        [InlineData("protobuf", true)]
        [InlineData("protobuf", false)]
        protected void Serialize_ThenDeserialize_ReturnIsSameAsInput(string serializerName, bool deserializeFromBytes)
        {
            var serializer = GetSerializer(serializerName);
            
            var input = new DummyClass
            {
                IntValue = 1,
                StringValue = "abc"
            };
            
            var stream = new MemoryStream();
            serializer.Serialize(stream, input);

            stream.Position = 0;
            DummyClass deserialized;
            if (deserializeFromBytes)
            {
                var bytes = stream.ToArray();
                deserialized = serializer.Deserialize(bytes);
            }
            else
            {
                deserialized = serializer.Deserialize(stream);
            }

            deserialized.Should().BeEquivalentTo(input);
        }

        private static IStreamSerializer<DummyClass> GetSerializer(string name)
        {
            return name switch
            {
                "json" => (IStreamSerializer<DummyClass>)new JsonSerializer<DummyClass>(),
                "protobuf" => new ProtoBufSerializer<DummyClass>(),
                _ => throw new Exception()
            };
        }
    }
}