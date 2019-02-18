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
            var decompressed = Decompress(compressed);
            return Encoding.UTF8.GetString(decompressed);
        }

        private static string Compress(string input)
        {
            var encoded = Encoding.UTF8.GetBytes(input);
            var compressed = Compress(encoded);
            return Convert.ToBase64String(compressed);
        }

        private static byte[] Decompress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                var lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source, CompressionMode.Decompress))
                {
                    var result = new byte[length];
                    decompressionStream.Read(result, 0, length);
                    return result;
                }
            }
        }

        private static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result, CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();
                }
                return result.ToArray();
            }
        }
    }
}