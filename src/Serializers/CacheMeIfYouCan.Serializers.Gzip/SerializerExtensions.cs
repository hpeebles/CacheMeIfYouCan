namespace CacheMeIfYouCan.Serializers.Gzip
{
    public static class SerializerExtensions
    {
        public static ISerializer WithGzipCompression(this ISerializer serializer, bool enabled = true)
        {
            return enabled
                ? new GzipSerializerWrapper(serializer)
                : serializer;
        }
    }
}