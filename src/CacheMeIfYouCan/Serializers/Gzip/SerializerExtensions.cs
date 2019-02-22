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
        
        public static IByteSerializer WithGzipCompression(this IByteSerializer serializer, bool enabled = true)
        {
            return enabled
                ? new GzipByteSerializerWrapper(serializer)
                : serializer;
        }
    }
}