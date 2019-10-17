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
        
        public CachedObjectConfigurationManager_WithRegularUpdates<T> WithRefreshInterval(TimeSpan onSuccessfulUpdate, TimeSpan onFailedUpdate)
        {
            return new CachedObjectConfigurationManager_WithRegularUpdates<T>(
                _initialiseValueFunc,
                null,
                onSuccessfulUpdate,
                onFailedUpdate);
        }

        public CachedObjectConfigurationManager<T, Unit> WithRefreshIntervalFactory(
            Func<ICachedObjectUpdateAttemptResult<T, Unit>, TimeSpan> refreshIntervalFunc)
        {
            return new CachedObjectConfigurationManager<T, Unit>(
                _initialiseValueFunc,
                null,
                new CachedObjectRegularIntervalWithJitterScheduler<T>(refreshIntervalFunc, 0));
        }
        
        public CachedObjectConfigurationManager<T, Unit> WithRefreshIntervalFactory(
            Func<CachedObjectSuccessfulUpdateResult<T, Unit>, TimeSpan> onSuccessfulUpdateIntervalFunc,
            Func<CachedObjectUpdateException<T, Unit>, TimeSpan> onFailedUpdateIntervalFunc)
        {
            return new CachedObjectConfigurationManager<T, Unit>(
                _initialiseValueFunc,
                null,
                new CachedObjectRegularIntervalWithJitterScheduler<T>(RefreshIntervalFactory, 0));

            TimeSpan RefreshIntervalFactory(ICachedObjectUpdateAttemptResult<T, Unit> result)
            {
                switch (result)
                {
                    case CachedObjectSuccessfulUpdateResult<T, Unit> success:
                        return onSuccessfulUpdateIntervalFunc(success);
                    
                    case CachedObjectUpdateException<T, Unit> failure:
                        return onFailedUpdateIntervalFunc(failure);
                    
                    default:
                        throw new InvalidOperationException();
                }
            }
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