namespace CacheMeIfYouCan.Internal
{
    internal static class Messages
    {
        public static string NoKeySerializerDefined<T>()
        {
            return $"No serializer defined for keys of type '{typeof(T).FullName}'. Use 'WithKeySerializer(...)' to set one.";
        }
        
        public static string NoKeyComparerDefined<T>()
        {
            return $"No IEqualityComparer defined for keys of type '{typeof(T).FullName}'. Use 'WithKeyComparer(...)' to set one.";
        }
    }
}