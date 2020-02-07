using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class CachedObject<T> : ICachedObject<T>
    {
        private readonly Func<CancellationToken, Task<T>> _getValueFunc;
        private readonly Func<TimeSpan> _refreshIntervalFactory;
        private readonly TimeSpan? _refreshValueFuncTimeout;
        private readonly CancellationTokenSource _isDisposedCancellationTokenSource;
        private readonly object _updateStateLock = new object();
        private TaskCompletionSource<bool> _initializationTaskCompletionSource;
        private T _value;
        private int _state;
        private DateTime _dateOfPreviousSuccessfulRefresh;
        private Timer _refreshTimer;
        private long _version;
        private const int PendingInitialization = (int)CachedObjectState.PendingInitialization;
        private const int InitializationInProgress = (int)CachedObjectState.InitializationInProgress;
        private const int Ready = (int)CachedObjectState.Ready;
        private const int Disposed = (int)CachedObjectState.Disposed;

        public CachedObject(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<TimeSpan> refreshIntervalFactory,
            TimeSpan? refreshValueFuncTimeout)
        {
            _getValueFunc = getValueFunc;
            _refreshIntervalFactory = refreshIntervalFactory;
            _refreshValueFuncTimeout = refreshValueFuncTimeout;
            _isDisposedCancellationTokenSource = new CancellationTokenSource();
        }

        public T Value
        {
            get
            {
                var state = _state;
                if (state == Ready)
                    return _value;
                
                ThrowIfDisposed();
                
                Task.Run(() => InitializeAsync()).GetAwaiter().GetResult();

                Thread.MemoryBarrier();
                return _value;
            }
        }

        public CachedObjectState State => (CachedObjectState)_state;
        public long Version => _version;
        public event EventHandler<CachedObjectValueRefreshedEvent<T>> OnValueRefreshed;
        public event EventHandler<CachedObjectValueRefreshExceptionEvent<T>> OnValueRefreshException;

        public void Initialize(CancellationToken cancellationToken = default)
        {
            Task.Run(() => InitializeAsync(cancellationToken)).GetAwaiter().GetResult();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<bool> tcs;
            var initializationAlreadyInProgress = false;
            lock (_updateStateLock)
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

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _isDisposedCancellationTokenSource.Token);
            
            if (_refreshValueFuncTimeout.HasValue)
                cts.CancelAfter(_refreshValueFuncTimeout.Value);
            
            cancellationToken = cts.Token;
            
            TimeSpan refreshInterval;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _value = await _getValueFunc(cancellationToken).ConfigureAwait(false);
                refreshInterval = _refreshIntervalFactory();
                tcs.TrySetResult(true);
                
                Interlocked.Increment(ref _version);

                PublishValueRefreshedEvent(default, stopwatch.Elapsed);
                
                _dateOfPreviousSuccessfulRefresh = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                lock (_updateStateLock)
                {
                    ThrowIfDisposed();
                    _state = PendingInitialization;
                    _initializationTaskCompletionSource = null;
                }
                
                tcs.TrySetException(ex);
                
                PublishValueRefreshExceptionEvent(ex, stopwatch.Elapsed);
                
                throw;
            }

            lock (_updateStateLock)
            {
                ThrowIfDisposed();
                _state = Ready;
                _initializationTaskCompletionSource = null;
            }

            _refreshTimer = new Timer(
                async _ => await RefreshValue().ConfigureAwait(false),
                null,
                (long)refreshInterval.TotalMilliseconds,
                -1);
        }

        public void Dispose()
        {
            lock (_updateStateLock)
            {
                if (_state == Disposed)
                    return;

                _state = Disposed;
                _isDisposedCancellationTokenSource.Cancel();

                var disposedTcs = new TaskCompletionSource<bool>();
                disposedTcs.SetException(GetObjectDisposedException());

                var initializationTcs = Interlocked.Exchange(ref _initializationTaskCompletionSource, disposedTcs);

                initializationTcs?.TrySetException(GetObjectDisposedException());

                _refreshTimer.Dispose();
            }
        }

        private async Task RefreshValue()
        {
            if (_state == Disposed)
                return;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_isDisposedCancellationTokenSource.Token);
            if (_refreshValueFuncTimeout.HasValue)
                cts.CancelAfter(_refreshValueFuncTimeout.Value);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var newValue = await _getValueFunc(cts.Token).ConfigureAwait(false);
                var oldValue = _value;
                _value = newValue;
                
                Interlocked.Increment(ref _version);
                
                PublishValueRefreshedEvent(oldValue, stopwatch.Elapsed);
                
                _dateOfPreviousSuccessfulRefresh = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                PublishValueRefreshExceptionEvent(ex, stopwatch.Elapsed);
            }
            finally
            {
                var nextInterval = _refreshIntervalFactory();

                lock (_updateStateLock)
                {
                    if (_state != Disposed)
                        _refreshTimer.Change((long)nextInterval.TotalMilliseconds, -1);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PublishValueRefreshedEvent(T previousValue, TimeSpan duration)
        {
            var onValueRefreshedEvent = OnValueRefreshed;
            if (onValueRefreshedEvent is null)
                return;
            
            var message = new CachedObjectValueRefreshedEvent<T>(
                _value,
                previousValue,
                duration,
                _dateOfPreviousSuccessfulRefresh,
                _version);

            onValueRefreshedEvent(this, message);
        }
        
        private void PublishValueRefreshExceptionEvent(Exception exception, TimeSpan duration)
        {
            var onValueRefreshExceptionEvent = OnValueRefreshException;
            if (onValueRefreshExceptionEvent is null)
                return;
            
            var message = new CachedObjectValueRefreshExceptionEvent<T>(
                exception,
                _value,
                duration,
                _dateOfPreviousSuccessfulRefresh,
                _version);

            onValueRefreshExceptionEvent(this, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (_state == Disposed)
                throw GetObjectDisposedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ObjectDisposedException GetObjectDisposedException()
        {
            return new ObjectDisposedException(this.GetType().ToString());
        }
    }
}