using System;
using System.IO;
using CacheMeIfYouCan.Serializers.Json;
using CacheMeIfYouCan.Serializers.MessagePack;
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
        [InlineData("messagepack", true)]
        [InlineData("messagepack", false)]
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

        private static ISerializer<DummyClass> GetSerializer(string name)
        {
            return name switch
            {
                "json" => (ISerializer<DummyClass>)new JsonSerializer<DummyClass>(),
                "messagepack" => new MessagePackSerializer<DummyClass>(),
                "protobuf" => new ProtoBufSerializer<DummyClass>(),
                _ => throw new Exception($"Unrecognised serializer name - {name}")
            };
        }
    }
}