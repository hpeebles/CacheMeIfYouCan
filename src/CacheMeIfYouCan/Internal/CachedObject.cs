using System;
using System.Collections.Generic;
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
        private readonly Action<CachedObjectSuccessfulUpdateResult<T, TUpdates>> _onValueUpdated;
        private readonly Action<CachedObjectUpdateException<T, TUpdates>> _onException;
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
        
        public CachedObject(
            Func<Task<T>> initialiseValueFunc,
            Func<T, TUpdates, Task<T>> updateValueFunc,
            ICachedObjectUpdateScheduler<T, TUpdates> updateScheduler,
            string name,
            Action<CachedObjectSuccessfulUpdateResult<T, TUpdates>> onValueUpdated,
            Action<CachedObjectUpdateException> onException)
        {
            _initialiseValueFunc = initialiseValueFunc ?? throw new ArgumentNullException(nameof(initialiseValueFunc));
            _updateValueFunc = updateValueFunc ?? throw new ArgumentNullException(nameof(updateValueFunc));
            _updateScheduler = updateScheduler;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _onValueUpdated = onValueUpdated;
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
                {
                    Initialize();
                }

                if (_state == CachedObjectState.Disposed)
                    throw new ObjectDisposedException(nameof(CachedObject<T, TUpdates>));
                
                return _value;
            }
        }

        public CachedObjectInitializeOutcome Initialize()
        {
            return Task.Run(InitializeAsync).GetAwaiter().GetResult();
        }

        public async Task<CachedObjectInitializeOutcome> InitializeAsync()
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
                    // Only throw exceptions that happen during initialization
                    var result = await UpdateValue((_, __) => _initialiseValueFunc(), default, true);

                    _updateScheduler?.Start(
                        (CachedObjectSuccessfulUpdateResult<T, TUpdates>)result,
                        u => UpdateValue(_updateValueFunc, u, false));

                    _state = CachedObjectState.Ready;
                }
                catch
                {
                    _state = CachedObjectState.PendingInitialization;
                    throw;
                }
            }
        }

        public ICachedObject<TOut, T> Map<TOut>(Func<T, TOut> converter, string name = null)
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
            if (_state == CachedObjectState.Disposed)
                return;
        
            _state = CachedObjectState.Disposed;
            CachedObjectInitializer.Remove(this);
            _cts.Cancel();
            _cts.Dispose();
        }
        
        private async Task<ICachedObjectUpdateAttemptResult<T, TUpdates>> UpdateValue(Func<T, TUpdates, Task<T>> updateFunc, TUpdates updates, bool throwException)
        {
            if (_state == CachedObjectState.Disposed)
                throw new ObjectDisposedException(nameof(CachedObject<T, TUpdates>));

            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            CachedObjectUpdateException<T, TUpdates> updateException = null;
            _updateAttemptCount++;

            try
            {
                var newValue = await updateFunc(_value, updates);
                
                if (_state == CachedObjectState.Disposed)
                    throw new ObjectDisposedException(nameof(CachedObject<T, TUpdates>));

                _value = newValue;
                _successfulUpdateCount++;

                var result = new CachedObjectSuccessfulUpdateResult<T, TUpdates>(
                    Name,
                    start,
                    StopwatchHelper.GetDuration(stopwatchStart),
                    newValue,
                    updates,
                    _updateAttemptCount,
                    _successfulUpdateCount,
                    _lastUpdateAttempt,
                    _lastSuccessfulUpdate);

                _onValueUpdated?.Invoke(result);
                OnValueUpdated?.Invoke(this, new CachedObjectSuccessfulUpdateResultEventArgs<T, TUpdates>(result));
                _valuesObservable.OnNext(newValue);

                return result;
            }
            catch (Exception ex)
            {
                updateException = new CachedObjectUpdateException<T, TUpdates>(
                    Name,
                    ex,
                    start,
                    StopwatchHelper.GetDuration(stopwatchStart),
                    _value,
                    updates,
                    _updateAttemptCount,
                    _successfulUpdateCount,
                    _lastUpdateAttempt,
                    _lastSuccessfulUpdate);

                _onException?.Invoke(updateException);
                OnException?.Invoke(this, new CachedObjectUpdateExceptionEventArgs<T, TUpdates>(updateException));

                if (throwException)
                    throw;

                return updateException;
            }
            finally
            {
                _lastUpdateAttempt = DateTime.UtcNow;

                if (updateException == null)
                    _lastSuccessfulUpdate = _lastUpdateAttempt;
            }
        }
        
        #region OnValueUpdated and OnException events
        
        private readonly List<(EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T>>, EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T, TUpdates>>)> _onValueUpdatedHandlers =
            new List<(EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T>>, EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T, TUpdates>>)>();
        
        private readonly List<(EventHandler<CachedObjectUpdateExceptionEventArgs<T>>, EventHandler<CachedObjectUpdateExceptionEventArgs<T, TUpdates>>)> _onExceptionHandlers =
            new List<(EventHandler<CachedObjectUpdateExceptionEventArgs<T>>, EventHandler<CachedObjectUpdateExceptionEventArgs<T, TUpdates>>)>();

        public event EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T, TUpdates>> OnValueUpdated;
        event EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T>> ICachedObject<T>.OnValueUpdated
        {
            add
            {
                EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T, TUpdates>> handler = (obj, args) => value(obj, args);
                lock (_onValueUpdatedHandlers)
                    _onValueUpdatedHandlers.Add((value, handler));

                OnValueUpdated += handler;
            }
            remove
            {
                EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T, TUpdates>> handler;
                lock (_onValueUpdatedHandlers)
                {
                    var index = _onValueUpdatedHandlers.FindIndex(x => x.Item1 == value);
                    if (index < 0)
                        return;
                    
                    handler = _onValueUpdatedHandlers[index].Item2;
                    _onValueUpdatedHandlers.RemoveAt(index);
                }

                OnValueUpdated -= handler;
            }
        }

        public event EventHandler<CachedObjectUpdateExceptionEventArgs<T, TUpdates>> OnException;
        event EventHandler<CachedObjectUpdateExceptionEventArgs<T>> ICachedObject<T>.OnException
        {
            add
            {
                EventHandler<CachedObjectUpdateExceptionEventArgs<T, TUpdates>> handler = (obj, args) => value(obj, args);
                lock (_onExceptionHandlers)
                    _onExceptionHandlers.Add((value, handler));

                OnException += handler;
            }
            remove
            {
                EventHandler<CachedObjectUpdateExceptionEventArgs<T, TUpdates>> handler;
                lock (_onExceptionHandlers)
                {
                    var index = _onExceptionHandlers.FindIndex(x => x.Item1 == value);
                    if (index < 0)
                        return;
                    
                    handler = _onExceptionHandlers[index].Item2;
                    _onExceptionHandlers.RemoveAt(index);
                }

                OnException -= handler;
            }
        }
        
        #endregion
    }
}