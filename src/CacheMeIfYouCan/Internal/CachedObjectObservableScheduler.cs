using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObjectObservableScheduler<T, TUpdates> : ICachedObjectUpdateScheduler<T, TUpdates>
    {
        private readonly IObservable<TUpdates> _observable;
        private IDisposable _subscription;
        
        public CachedObjectObservableScheduler(IObservable<TUpdates> observable)
        {
            _observable = observable;
        }
        
        public void Start(
            CachedObjectSuccessfulUpdateResult<T, TUpdates> initialiseResult,
            Func<TUpdates, Task<ICachedObjectUpdateAttemptResult<T, TUpdates>>> updateValueFunc)
        {
            _subscription = _observable
                .Select(x => Observable.FromAsync(() => updateValueFunc(x)))
                .Concat()
                .Retry()
                .Subscribe();
        }

        public void Dispose() => _subscription?.Dispose();
    }
}