using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface ICachedObject<out T>
    {
        T Value { get; }

        Task<bool> Init();
    }
}