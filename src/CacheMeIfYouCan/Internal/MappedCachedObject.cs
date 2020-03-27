using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class MappedCachedObject<TSource, T> : CachedObjectBase<T>, ICachedObject<T>
    {
        private readonly ICachedObject<TSource> _source;
        private readonly Func<TSource, ValueTask<T>> _map;
        private readonly object _lock = new Object();
        private long _sourceVersion;
        private TaskCompletionSource<T> _initializationTcs;

        public MappedCachedObject(ICachedObject<TSource> source, Func<TSource, T> map)
            : this(source, x => new ValueTask<T>(map(x)))
        { }
        
        public MappedCachedObject(ICachedObject<TSource> source, Func<TSource, Task<T>> map)
            : this(source, x => new ValueTask<T>(map(x)))
        { }
        
        public MappedCachedObject(ICachedObject<TSource> source, Func<TSource, ValueTask<T>> map)
        {
            _source = source;
            _map = map;
            
            _source.OnValueRefreshed += async (_, args) => await UpdateValue(args.NewValue, args.Version).ConfigureAwait(false);
        }

        protected override async Task<T> GetInitialValue(CancellationToken cancellationToken)
        {
            var sourceState = _source.State;
            if (sourceState == CachedObjectState.PendingInitialization ||
                sourceState == CachedObjectState.InitializationInProgress)
            {
                await _source.InitializeAsync(cancellationToken).ConfigureAwait(false);
            }

            var valueTask = _map(_source.Value);

            return valueTask.IsCompleted
                ? valueTask.Result
                : await valueTask.ConfigureAwait(false);
        }

        public override void RefreshValue(
            TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task RefreshValueAsync(
            TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async ValueTask UpdateValue(TSource sourceValue, long sourceVersion)
        {
            if (!IsReady())
                return;

            if (sourceVersion <= Volatile.Read(ref _sourceVersion))
                return;

            var timer = Stopwatch.StartNew();
            try
            {
                var valueTask = _map(sourceValue);
                var value = valueTask.IsCompleted
                    ? valueTask.Result
                    : await valueTask.ConfigureAwait(false);
                
                var previousValue = _value;
                lock (_lock)
                {
                    if (sourceVersion <= _sourceVersion)
                        return;

                    _value = value;
                    _version++;
                    _sourceVersion = sourceVersion;
                    _initializationTcs?.TrySetResult(value);
                }

                PublishValueRefreshedEvent(previousValue, timer.Elapsed);

                _datePreviousSuccessfulRefreshFinished = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                lock (_lock)
                    _initializationTcs?.TrySetException(ex);

                PublishValueRefreshExceptionEvent(ex, timer.Elapsed);
            }
            finally
            {
                _initializationTcs = null;
            }
        }
    }
}