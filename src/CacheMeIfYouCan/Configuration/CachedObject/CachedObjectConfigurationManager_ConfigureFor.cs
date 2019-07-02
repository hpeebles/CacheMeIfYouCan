using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration.CachedObject
{
    public class CachedObjectConfigurationManager_ConfigureFor<T>
    {
        private readonly Func<Task<T>> _initialiseValueFunc;

        internal CachedObjectConfigurationManager_ConfigureFor(Func<Task<T>> initialiseValueFunc)
        {
            _initialiseValueFunc = initialiseValueFunc;
        }

        public CachedObjectConfigurationManager_WithRegularUpdates<T> WithRefreshInterval(TimeSpan interval)
        {
            return WithRefreshInterval(interval, interval);
        }
        
        public CachedObjectConfigurationManager_WithRegularUpdates<T> WithRefreshInterval(TimeSpan onSuccess, TimeSpan onFailure)
        {
            return new CachedObjectConfigurationManager_WithRegularUpdates<T>(
                _initialiseValueFunc,
                null,
                onSuccess,
                onFailure);
        }

        public CachedObjectConfigurationManager<T, Unit> WithRefreshIntervalFactory(
            Func<CachedObjectUpdateResult<T, Unit>, TimeSpan> refreshIntervalFunc)
        {
            return new CachedObjectConfigurationManager<T, Unit>(
                _initialiseValueFunc,
                null,
                new CachedObjectRegularIntervalWithJitterScheduler<T>(refreshIntervalFunc, 0));
        }
        
        public CachedObjectConfigurationManager<T, Unit> RefreshOnEach<TAny>(IObservable<TAny> observable)
        {
            return new CachedObjectConfigurationManager<T, Unit>(
                _initialiseValueFunc,
                null,
                new CachedObjectObservableScheduler<T, Unit>(observable.Select(_ => Unit.Instance)));
        }

        public CachedObjectConfigurationManager_WithRegularUpdates<T> WithUpdates(TimeSpan interval, Func<T, T> updateValueFunc)
        {
            return WithUpdates(interval, x => Task.Run(() => updateValueFunc(x)));
        }
        
        public CachedObjectConfigurationManager_WithRegularUpdates<T> WithUpdates(TimeSpan interval, Func<T, Task<T>> updateValueFunc)
        {
            return new CachedObjectConfigurationManager_WithRegularUpdates<T>(
                _initialiseValueFunc,
                (value, _) => updateValueFunc(value),
                interval,
                interval);
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
                _initialiseValueFunc,
                updateValueFunc,
                new CachedObjectObservableScheduler<T, TUpdates>(observable));
        }

        public CachedObjectConfigurationManager<T, Unit> WithNoUpdates()
        {
            return new CachedObjectConfigurationManager<T, Unit>(_initialiseValueFunc);
        }
    }
}