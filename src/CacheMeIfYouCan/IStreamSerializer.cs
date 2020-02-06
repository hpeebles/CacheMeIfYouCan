using System.IO;

namespace CacheMeIfYouCan
{
    public interface IStreamSerializer<T>
    {
        void WriteToStream(Stream stream, T obj);

        T Deserialize(Stream stream);

        T Deserialize(byte[] bytes);
    }
}