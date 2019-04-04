using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal interface ICachedObjectUpdateScheduler<T, TUpdates> : IDisposable
    {
        void Start(
            CachedObjectUpdateResult<T, TUpdates> initialiseResult,
            Func<TUpdates, Task<CachedObjectUpdateResult<T, TUpdates>>> updateValueFunc);
    }
}