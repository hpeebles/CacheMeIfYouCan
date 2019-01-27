namespace CacheMeIfYouCan
{
    public interface IKeyValuePair<out TK, out TV>
    {
        TK Key { get; }
        TV Value { get; }
    }
}