using System.IO;

namespace CacheMeIfYouCan
{
    public interface ISerializer<T>
    {
        void Serialize(Stream destination, T value);

        T Deserialize(Stream source);

        T Deserialize(byte[] bytes);
    }
}