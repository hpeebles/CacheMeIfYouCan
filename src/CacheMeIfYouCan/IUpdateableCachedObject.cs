using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface IUpdateableCachedObject<T, TUpdates> : ICachedObject<T, TUpdates>
    {
        void UpdateValue(TUpdates updates);
        Task UpdateValueAsync(TUpdates updates, CancellationToken cancellationToken = default);
    }
}