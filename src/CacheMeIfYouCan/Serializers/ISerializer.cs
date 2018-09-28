namespace CacheMeIfYouCan.Serializers
{
    public interface ISerializer
    {
        string Serialize<T>(T value);

        T Deserialize<T>(string value);
    }

    public interface ISerializer<T>
    {
        string Serialize(T value);
        
        T Deserialize(string value);
    }
}