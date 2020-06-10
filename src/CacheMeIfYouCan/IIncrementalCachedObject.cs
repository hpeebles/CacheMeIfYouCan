using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface IIncrementalCachedObject<T, TUpdates> : ICachedObject<T, TUpdates>
    {
        void UpdateValue();
        Task UpdateValueAsync(CancellationToken cancellationToken = default);
    }
}