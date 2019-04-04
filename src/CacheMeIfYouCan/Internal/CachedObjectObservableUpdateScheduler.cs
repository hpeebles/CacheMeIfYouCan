using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObjectObservableUpdateScheduler<T, TUpdates> : ICachedObjectUpdateScheduler<T, TUpdates>
    {
        private readonly IObservable<TUpdates> _observable;
        private readonly CancellationTokenSource _cts;
        
        public CachedObjectObservableUpdateScheduler(IObservable<TUpdates> observable)
        {
            _observable = observable;
            _cts = new CancellationTokenSource();
        }
        
        public void Start(
            CachedObjectUpdateResult<T, TUpdates> initialiseResult,
            Func<TUpdates, Task<CachedObjectUpdateResult<T, TUpdates>>> updateValueFunc)
        {
            _observable
                .SelectMany(updateValueFunc)
                .Retry()
                .Subscribe(_cts.Token);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}