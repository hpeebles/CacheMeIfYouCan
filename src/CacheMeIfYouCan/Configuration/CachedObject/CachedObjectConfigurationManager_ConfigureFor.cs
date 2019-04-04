using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration.CachedObject
{
    public class CachedObjectConfigurationManager_ConfigureFor<T> : CachedObjectConfigurationManager<T, Unit>
    {
        internal CachedObjectConfigurationManager_ConfigureFor(Func<Task<T>> getValueFunc)
            : base(getValueFunc)
        {}

        public CachedObjectConfigurationManager_WithRefreshInterval<T> WithRefreshInterval(TimeSpan interval)
        {
            return WithRefreshInterval(interval, interval);
        }
        
        public CachedObjectConfigurationManager_WithRefreshInterval<T> WithRefreshInterval(TimeSpan onSuccess, TimeSpan onFailure)
        {
            return new CachedObjectConfigurationManager_WithRefreshInterval<T>(
                InitialiseValueFunc,
                onSuccess,
                onFailure);
        }

        public CachedObjectConfigurationManager<T, Unit> WithRefreshIntervalFactory(
            Func<CachedObjectUpdateResult<T, Unit>, TimeSpan> refreshIntervalFunc)
        {
            return new CachedObjectConfigurationManager<T, Unit>(
                InitialiseValueFunc,
                null,
                new CachedObjectRefreshWithJitterScheduler<T>(refreshIntervalFunc, 0));
        }
        
        public CachedObjectConfigurationManager<T, Unit> RefreshOn<TAny>(IObservable<TAny> observable)
        {
            return new CachedObjectConfigurationManager<T, Unit>(
                InitialiseValueFunc,
                null,
                new CachedObjectObservableUpdateScheduler<T, Unit>(observable.Select(_ => Unit.Instance)));
        }
        
        public CachedObjectConfigurationManager<T, TUpdates> WithUpdates<TUpdates>(
            IObservable<TUpdates> observable,
            Func<T, TUpdates, T> updateValueFunc)
        {
            return WithUpdates(observable, (v, u) => Task.Run(() => updateValueFunc(v, u)));
        }
        
        public CachedObjectConfigurationManager<T, TUpdates> WithUpdates<TUpdates>(
            IObservable<TUpdates> observable,
            Func<T, TUpdates, Task<T>> updateValueFunc)
        {
            return new CachedObjectConfigurationManager<T, TUpdates>(
                InitialiseValueFunc,
                updateValueFunc,
                new CachedObjectObservableUpdateScheduler<T, TUpdates>(observable));
        }
    }
}