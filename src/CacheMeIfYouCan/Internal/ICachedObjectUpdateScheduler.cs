using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal interface ICachedObjectUpdateScheduler<T, TUpdates> : IDisposable
    {
        void Start(
            CachedObjectSuccessfulUpdateResult<T, TUpdates> initialiseResult,
            Func<TUpdates, Task<ICachedObjectUpdateAttemptResult<T, TUpdates>>> updateValueFunc);
    }
}