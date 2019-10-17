using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObjectObservableScheduler<T, TUpdates> : ICachedObjectUpdateScheduler<T, TUpdates>
    {
        private readonly IObservable<TUpdates> _observable;
        private readonly CancellationTokenSource _cts;
        
        public CachedObjectObservableScheduler(IObservable<TUpdates> observable)
        {
            _observable = observable;
            _cts = new CancellationTokenSource();
        }
        
        public void Start(
            CachedObjectSuccessfulUpdateResult<T, TUpdates> initialiseResult,
            Func<TUpdates, Task<ICachedObjectUpdateAttemptResult<T, TUpdates>>> updateValueFunc)
        {
            _observable
                .Select(x => Observable.FromAsync(() => updateValueFunc(x)))
                .Concat()
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