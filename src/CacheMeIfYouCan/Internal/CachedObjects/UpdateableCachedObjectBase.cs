using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal.CachedObjects
{
    internal abstract class UpdateableCachedObjectBase<T, TUpdates> : CachedObject<T>, ICachedObject<T, TUpdates>
    {
        protected UpdateableCachedObjectBase(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<TimeSpan> refreshIntervalFactory,
            TimeSpan? refreshValueFuncTimeout)
            : base(getValueFunc, refreshIntervalFactory, refreshValueFuncTimeout)
        { }
        
        public new ICachedObject<TOut, TUpdates> Map<TOut>(Func<T, TOut> map)
        {
            return MapAsync(v => Task.FromResult(map(v)));
        }

        public new ICachedObject<TOut, TUpdates> MapAsync<TOut>(Func<T, Task<TOut>> map)
        {
            return new MappedCachedObject<T, TUpdates, TOut>(this, map);
        }
        
        public ICachedObject<TOut, TUpdates> Map<TOut>(Func<T, TOut> map, Func<TOut, T, TUpdates, TOut> mapUpdatesFunc)
        {
            return MapAsync(
                sourceValue => Task.FromResult(map(sourceValue)),
                (value, sourceValue, updates) => Task.FromResult(mapUpdatesFunc(value, sourceValue, updates)));
        }

        public ICachedObject<TOut, TUpdates> MapAsync<TOut>(
            Func<T, Task<TOut>> map,
            Func<TOut, T, TUpdates, Task<TOut>> mapUpdatesFunc)
        {
            return new MappedCachedObject<T, TUpdates, TOut>(this, map, mapUpdatesFunc);
        }

        public event EventHandler<ValueUpdatedEvent<T, TUpdates>> OnValueUpdated;
        public event EventHandler<ValueUpdateExceptionEvent<T, TUpdates>> OnValueUpdateException;
        
        private protected async Task UpdateValueWithinLock(
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc,
            TUpdates updates,
            CancellationToken cancellationToken)
        {
            var stopwatch = StopwatchStruct.StartNew();
            try
            {
                var previousValue = _value;
                _value = await applyUpdatesFunc(_value, updates, cancellationToken).ConfigureAwait(false);
                
                Interlocked.Increment(ref _version);

                PublishValueUpdatedEvent(previousValue, updates, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                PublishValueUpdateExceptionEvent(ex, updates, stopwatch.Elapsed);
                throw;
            }
            finally
            {
                ReleaseRefreshOrUpdateValueLock();
            }
        }
        
        private void PublishValueUpdatedEvent(T previousValue, TUpdates updates, TimeSpan duration)
        {
            var onValueUpdatedEvent = OnValueUpdated;
            if (onValueUpdatedEvent is null)
                return;
            
            var message = new ValueUpdatedEvent<T, TUpdates>(
                _value,
                previousValue,
                updates,
                duration,
                _version);

            onValueUpdatedEvent(this, message);
        }
        
        private void PublishValueUpdateExceptionEvent(Exception exception, TUpdates updates, TimeSpan duration)
        {
            var onValueUpdateExceptionEvent = OnValueUpdateException;
            if (onValueUpdateExceptionEvent is null)
                return;
                     
            var message = new ValueUpdateExceptionEvent<T, TUpdates>(
                exception,
                _value,
                updates,
                duration,
                _version);
         
            onValueUpdateExceptionEvent(this, message);
        }
    }
}