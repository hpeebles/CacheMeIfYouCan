using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObject<T, TUpdates> : ICachedObject<T>
    {
        private readonly Func<Task<T>> _initialiseValueFunc;
        private readonly Func<T, TUpdates, Task<T>> _updateValueFunc;
        private readonly Action<CachedObjectUpdateResult<T, TUpdates>> _onUpdate;
        private readonly Action<CachedObjectUpdateException> _onException;
        private readonly ICachedObjectUpdateScheduler<T, TUpdates> _updateScheduler;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _cts;
        private int _updateAttemptCount;
        private int _successfulUpdateCount;
        private DateTime _lastUpdateAttempt;
        private DateTime _lastSuccessfulUpdate;
        private T _value;
        private int _state;

        public CachedObject(
            Func<Task<T>> initialiseValueFunc,
            Func<T, TUpdates, Task<T>> updateValueFunc,
            string name,
            Action<CachedObjectUpdateResult<T, TUpdates>> onUpdate,
            Action<CachedObjectUpdateException> onException,
            ICachedObjectUpdateScheduler<T, TUpdates> updateScheduler)
        {
            _initialiseValueFunc = initialiseValueFunc ?? throw new ArgumentNullException(nameof(initialiseValueFunc));
            _updateValueFunc = updateValueFunc ?? throw new ArgumentNullException(nameof(updateValueFunc));
            Name = name;
            _onUpdate = onUpdate;
            _onException = onException;
            _updateScheduler = updateScheduler;
            _semaphore = new SemaphoreSlim(1);
            _cts = new CancellationTokenSource();
        }
        
        public string Name { get; }

        public T Value
        {
            get
            {
                if (_state == 1)
                    return _value;
                
                if (_state == 0)
                    Task.Run(Initialize).GetAwaiter().GetResult();
                
                if (_state == 2)
                    throw new ObjectDisposedException(nameof(CachedObject<T, TUpdates>));
                
                return _value;
            }
        }

        public async Task<CachedObjectInitializeOutcome> Initialize()
        {
            if (_state == 0)
            {
                await _semaphore.WaitAsync();

                try
                {
                    if (_state == 0)
                        await InitializeWithinLock();
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            switch (_state)
            {
                case 1:
                    return CachedObjectInitializeOutcome.Success;
                
                case 2:
                    return CachedObjectInitializeOutcome.Disposed;

                default:
                    return CachedObjectInitializeOutcome.Failure;
            }

            async Task InitializeWithinLock()
            {
                var result = await UpdateValueImpl((_, __) => _initialiseValueFunc(), default);

                if (!result.Success)
                    return;

                _updateScheduler.Start(result, UpdateValue);
                
                Interlocked.Exchange(ref _state, 1);
            }
        }

        public async Task<CachedObjectUpdateResult<T, TUpdates>> UpdateValue(TUpdates updates)
        {
            return await UpdateValueImpl(_updateValueFunc, updates);
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _state, 2);
            CachedObjectInitializer.Remove(this);
            _cts.Cancel();
            _cts.Dispose();
        }
        
        private async Task<CachedObjectUpdateResult<T, TUpdates>> UpdateValueImpl(Func<T, TUpdates, Task<T>> updateFunc, TUpdates updates)
        {
            if (_state == 2)
                throw new ObjectDisposedException(nameof(CachedObject<T, TUpdates>));

            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            CachedObjectUpdateException updateException = null;
            T newValue;
            _updateAttemptCount++;

            try
            {
                newValue = await updateFunc(_value, updates);
                _value = newValue;
                _successfulUpdateCount++;
            }
            catch (Exception ex)
            {
                updateException = new CachedObjectUpdateException(Name, ex);
                newValue = default;
                
                _onException?.Invoke(updateException);
            }
            
            var result = new CachedObjectUpdateResult<T, TUpdates>(
                Name,
                start,
                StopwatchHelper.GetDuration(stopwatchStart),
                newValue,
                updates,
                updateException,
                _updateAttemptCount,
                _successfulUpdateCount,
                _lastUpdateAttempt,
                _lastSuccessfulUpdate);

            _lastUpdateAttempt = DateTime.UtcNow;
            
            _onUpdate?.Invoke(result);
            
            if (updateException == null)
                _lastSuccessfulUpdate = DateTime.UtcNow;

            return result;
        }
    }
}