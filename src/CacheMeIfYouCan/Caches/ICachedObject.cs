using System.Threading.Tasks;

namespace CacheMeIfYouCan.Caches
{
    public interface ICachedObject<out T>
    {
        T Value { get; }

        Task<bool> Init();
    }
}