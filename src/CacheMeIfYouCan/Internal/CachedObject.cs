using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObject<T, TUpdates> : ICachedObject<T, TUpdates>
    {
        private readonly Func<Task<T>> _initialiseValueFunc;
        private readonly Func<T, TUpdates, Task<T>> _updateValueFunc;
        private readonly Action<CachedObjectUpdateResult<T, TUpdates>> _onUpdate;
        private readonly Action<CachedObjectUpdateException> _onException;
        private readonly ICachedObjectUpdateScheduler<T, TUpdates> _updateScheduler;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _cts;
        private readonly Subject<T> _valuesObservable;
        private int _updateAttemptCount;
        private int _successfulUpdateCount;
        private DateTime _lastUpdateAttempt;
        private DateTime _lastSuccessfulUpdate;
        private T _value;
        private volatile CachedObjectState _state;
        public event EventHandler<CachedObjectUpdateResultEventArgs<T, TUpdates>> OnUpdate;
        public event EventHandler<CachedObjectUpdateExceptionEventArgs> OnException;
        
        public CachedObject(
            Func<Task<T>> initialiseValueFunc,
            Func<T, TUpdates, Task<T>> updateValueFunc,
            ICachedObjectUpdateScheduler<T, TUpdates> updateScheduler,
            string name,
            Action<CachedObjectUpdateResult<T, TUpdates>> onUpdate,
            Action<CachedObjectUpdateException> onException)
        {
            _initialiseValueFunc = initialiseValueFunc ?? throw new ArgumentNullException(nameof(initialiseValueFunc));
            _updateValueFunc = updateValueFunc ?? throw new ArgumentNullException(nameof(updateValueFunc));
            _updateScheduler = updateScheduler;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _onUpdate = onUpdate;
            _onException = onException;
            _semaphore = new SemaphoreSlim(1);
            _cts = new CancellationTokenSource();
            _valuesObservable = new Subject<T>();
            _state = CachedObjectState.PendingInitialization;
        }
        
        public string Name { get; }
        public CachedObjectState State => _state;

        public T Value
        {
            get
            {
                if (_state == CachedObjectState.Ready)
                    return _value;
                
                if (_state == CachedObjectState.PendingInitialization ||
                    _state == CachedObjectState.InitializationInProgress)
                    Task.Run(Initialize).GetAwaiter().GetResult();
                
                if (_state == CachedObjectState.Disposed)
                    throw new ObjectDisposedException(nameof(CachedObject<T, TUpdates>));
                
                return _value;
            }
        }

        public async Task<CachedObjectInitializeOutcome> Initialize()
        {
            if (_state == CachedObjectState.PendingInitialization ||
                _state == CachedObjectState.InitializationInProgress)
            {
                await _semaphore.WaitAsync();

                try
                {
                    if (_state == CachedObjectState.PendingInitialization)
                        await InitializeWithinLock();
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            switch (_state)
            {
                case CachedObjectState.Ready:
                    return CachedObjectInitializeOutcome.Success;
                
                case CachedObjectState.Disposed:
                    return CachedObjectInitializeOutcome.Disposed;

                default:
                    return CachedObjectInitializeOutcome.Failure;
            }

            async Task InitializeWithinLock()
            {
                _state = CachedObjectState.InitializationInProgress;

                try
                {
                    var result = await UpdateValue((_, __) => _initialiseValueFunc(), default);

                    if (!result.Success)
                    {
                        _state = CachedObjectState.PendingInitialization;
                        return;
                    }

                    _updateScheduler?.Start(result, u => UpdateValue(_updateValueFunc, u));

                    _state = CachedObjectState.Ready;
                }
                catch
                {
                    _state = CachedObjectState.PendingInitialization;
                    throw;
                }
            }
        }

        public ICachedObject<TOut> Map<TOut>(Func<T, TOut> converter, string name = null)
        {
            if (name == null)
                name = $"{nameof(CachedObject<T, TUpdates>)}_{TypeNameHelper.GetNameIncludingInnerGenericTypeNames(typeof(T))}";
            
            return new CachedObject<TOut, T>(
                () => Task.FromResult(converter(this.Value)),
                (_, latest) => Task.FromResult(converter(latest)),
                new CachedObjectObservableScheduler<TOut, T>(_valuesObservable),
                name,
                null,
                null);
        }

        public void Dispose()
        {
            _state = CachedObjectState.Disposed;
            CachedObjectInitializer.Remove(this);
            _cts.Cancel();
            _cts.Dispose();
        }
        
        private async Task<CachedObjectUpdateResult<T, TUpdates>> UpdateValue(Func<T, TUpdates, Task<T>> updateFunc, TUpdates updates)
        {
            if (_state == CachedObjectState.Disposed)
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
                OnException?.Invoke(this, new CachedObjectUpdateExceptionEventArgs(updateException));
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
            OnUpdate?.Invoke(this, new CachedObjectUpdateResultEventArgs<T, TUpdates>(result));
            _valuesObservable.OnNext(newValue);

            if (updateException == null)
                _lastSuccessfulUpdate = DateTime.UtcNow;

            return result;
        }
    }
}