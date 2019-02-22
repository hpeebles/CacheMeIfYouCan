namespace CacheMeIfYouCan.Serializers
{
    public interface IByteSerializer
    {
        byte[] Serialize<T>(T value);

        T Deserialize<T>(byte[] value);
    }

    public interface IByteSerializer<T>
    {
        byte[] Serialize(T value);
        
        T Deserialize(byte[] value);
    }
}