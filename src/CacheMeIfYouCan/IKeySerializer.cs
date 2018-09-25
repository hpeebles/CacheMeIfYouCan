namespace CacheMeIfYouCan
{
    public interface IKeySerializer
    {
        string Serialize<T>(T value);
    }

    public interface IKeySerializer<in T>
    {
        string Serialize(T value);
    }
}