using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal
{
    internal abstract class CachedObjectBase<T> : ICachedObject<T>
    {
        protected readonly CancellationTokenSource _isDisposedCancellationTokenSource = new CancellationTokenSource();
        protected T _value;
        protected long _version;
        protected DateTime _datePreviousSuccessfulRefreshStarted;
        protected DateTime _datePreviousSuccessfulRefreshFinished;
        private readonly List<IDisposable> _toDispose = new List<IDisposable>();
        private readonly object _lock = new object();
        private TaskCompletionSource<bool> _initializationTaskCompletionSource;
        private volatile int _state;
        private const int PendingInitialization = (int)CachedObjectState.PendingInitialization;
        private const int InitializationInProgress = (int)CachedObjectState.InitializationInProgress;
        private const int Ready = (int)CachedObjectState.Ready;
        private const int Disposed = (int)CachedObjectState.Disposed;

        public T Value
        {
            get
            {
                var value = _value;
                if (_state == Ready)
                    return value;
                
                ThrowIfDisposed();
                
                Initialize();

                Thread.MemoryBarrier();
                return _value;
            }
        }

        public CachedObjectState State => (CachedObjectState)_state;
        public long Version => _version;
        public event EventHandler OnInitialized;
        public event EventHandler OnDisposed;
        public event EventHandler<ValueRefreshedEvent<T>> OnValueRefreshed;
        public event EventHandler<ValueRefreshExceptionEvent<T>> OnValueRefreshException;
        
        public void Initialize(CancellationToken cancellationToken = default)
        {
            Task.Run(() => InitializeAsync(cancellationToken), cancellationToken).GetAwaiter().GetResult();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<bool> tcs;
            var initializationAlreadyInProgress = false;
            lock (_lock)
            {
                switch (_state)
                {
                    case Disposed:
                        throw GetObjectDisposedException();

                    case Ready:
                        return;

                    case InitializationInProgress:
                        tcs = _initializationTaskCompletionSource;
                        initializationAlreadyInProgress = true;
                        break;

                    case PendingInitialization:
                        tcs = _initializationTaskCompletionSource = new TaskCompletionSource<bool>();
                        _state = InitializationInProgress;
                        break;
                    
                    default:
                        throw new InvalidOperationException("Invalid state - " + _state);
                }
            }

            if (initializationAlreadyInProgress)
            {
                await tcs.Task.ConfigureAwait(false);
                return;
            }

            var start = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _value = await GetInitialValue(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    _state = PendingInitialization;
                    _initializationTaskCompletionSource = null;
                }
                
                PublishValueRefreshExceptionEvent(ex, stopwatch.Elapsed);
                tcs.TrySetException(ex);

                throw;
            }

            lock (_lock)
            {
                ThrowIfDisposed();
                _version = 1;
                _state = Ready;
                _initializationTaskCompletionSource = null;
            }

            OnInitialized?.Invoke(this, null);
            tcs.TrySetResult(true);

            PublishValueRefreshedEvent(default, stopwatch.Elapsed);

            _datePreviousSuccessfulRefreshStarted = start;
            _datePreviousSuccessfulRefreshFinished = DateTime.UtcNow;
            
            PostInitializationAction();
        }

        protected abstract Task<T> GetInitialValue(CancellationToken cancellationToken);

        protected virtual void PostInitializationAction() { }

        protected void RegisterDisposable(IDisposable disposable) => _toDispose.Add(disposable);

        public ICachedObject<TOut> Map<TOut>(Func<T, TOut> map) => new MappedCachedObject<T, TOut>(this, map);
        public ICachedObject<TOut> MapAsync<TOut>(Func<T, Task<TOut>> map) => new MappedCachedObject<T, TOut>(this, map);

        public virtual void RefreshValue(
            TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default,
            CancellationToken cancellationToken = default)
        {
            Task.Run(() => RefreshValueAsync(
                skipIfPreviousRefreshStartedWithinTimeFrame,
                cancellationToken), cancellationToken).GetAwaiter().GetResult();
        }

        public abstract Task RefreshValueAsync(
            TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default,
            CancellationToken cancellationToken = default);
        
        public virtual void Dispose()
        {
            T finalValue;
            lock (_lock)
            {
                if (_state == Disposed)
                    return;

                _state = Disposed;
                _isDisposedCancellationTokenSource.Cancel();
                _isDisposedCancellationTokenSource.Dispose();

                var objectDisposedException = GetObjectDisposedException();
                
                var disposedTcs = new TaskCompletionSource<bool>();
                disposedTcs.SetException(objectDisposedException);

                var initializationTcs = Interlocked.Exchange(ref _initializationTaskCompletionSource, disposedTcs);

                initializationTcs?.TrySetException(objectDisposedException);

                finalValue = _value;
                _value = default;
            }
            
            foreach (var item in _toDispose)
                item.Dispose();

            if (finalValue is IDisposable disposable)
                disposable.Dispose();
            
            OnDisposed?.Invoke(this, null);
        }
        
        protected void PublishValueRefreshedEvent(T previousValue, TimeSpan duration)
        {
            var onValueRefreshedEvent = OnValueRefreshed;
            if (onValueRefreshedEvent is null)
                return;
            
            var message = new ValueRefreshedEvent<T>(
                _value,
                previousValue,
                duration,
                _datePreviousSuccessfulRefreshFinished,
                _version);

            onValueRefreshedEvent(this, message);
        }
        
        protected void PublishValueRefreshExceptionEvent(Exception exception, TimeSpan duration)
        {
            var onValueRefreshExceptionEvent = OnValueRefreshException;
            if (onValueRefreshExceptionEvent is null)
                return;
            
            var message = new ValueRefreshExceptionEvent<T>(
                exception,
                _value,
                duration,
                _datePreviousSuccessfulRefreshFinished,
                _version);

            onValueRefreshExceptionEvent(this, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsReady() => _state == Ready;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsDisposed() => _state == Disposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfDisposed()
        {
            if (IsDisposed())
                throw GetObjectDisposedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ObjectDisposedException GetObjectDisposedException()
        {
            return new ObjectDisposedException(this.GetType().ToString());
        }
    }
}