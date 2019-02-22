using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CacheMeIfYouCan.Serializers.Gzip
{
    internal class GzipSerializerWrapper : ISerializer
    {
        private readonly ISerializer _innerSerializer;

        public GzipSerializerWrapper(ISerializer innerSerializer)
        {
            _innerSerializer = innerSerializer;
        }

        public string Serialize<T>(T value)
        {
            var uncompressed = _innerSerializer.Serialize(value);

            return Compress(uncompressed);
        }

        public T Deserialize<T>(string value)
        {
            var uncompressed = Decompress(value);

            return _innerSerializer.Deserialize<T>(uncompressed);
        }
        
        private static string Decompress(string input)
        {
            var compressed = Convert.FromBase64String(input);
            var decompressed = CompressionHandler.Decompress(compressed);
            return Encoding.UTF8.GetString(decompressed);
        }

        private static string Compress(string input)
        {
            var encoded = Encoding.UTF8.GetBytes(input);
            var compressed = CompressionHandler.Compress(encoded);
            return Convert.ToBase64String(compressed);
        }
    }
}