namespace CacheMeIfYouCan.Serializers.Gzip
{
    internal class GzipByteSerializerWrapper : IByteSerializer
    {
        private readonly IByteSerializer _innerSerializer;

        public GzipByteSerializerWrapper(IByteSerializer innerSerializer)
        {
            _innerSerializer = innerSerializer;
        }

        public byte[] Serialize<T>(T value)
        {
            var uncompressed = _innerSerializer.Serialize(value);

            return CompressionHandler.Compress(uncompressed);
        }

        public T Deserialize<T>(byte[] value)
        {
            var uncompressed = CompressionHandler.Decompress(value);

            return _innerSerializer.Deserialize<T>(uncompressed);
        }
    }
}