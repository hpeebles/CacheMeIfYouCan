namespace CacheMeIfYouCan
{
    public interface IKeyValuePair<out TK, out TV>
    {
        TK Key { get; }
        TV Value { get; }
    }

    public static class KeyValuePairExtensions
    {
        public static void Deconstruct<TK, TV>(this IKeyValuePair<TK, TV> kv, out TK key, out TV value)
        {
            key = kv.Key;
            value = kv.Value;
        }
    }
}