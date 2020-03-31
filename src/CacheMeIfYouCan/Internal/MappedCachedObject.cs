using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class MappedCachedObject<TSource, T> : CachedObjectBase<T>, ICachedObject<T>
    {
        private readonly ICachedObject<TSource> _source;
        private readonly Func<TSource, ValueTask<T>> _map;
        private readonly Type _sourceUpdateFuncInputType;
        private readonly object _lock = new Object();
        private long _sourceVersion;
        private TaskCompletionSource<T> _initializationTcs;

        public MappedCachedObject(ICachedObject<TSource> source, Func<TSource, T> map)
            : this(source, x => new ValueTask<T>(map(x)))
        { }
        
        public MappedCachedObject(ICachedObject<TSource> source, Func<TSource, Task<T>> map)
            : this(source, x => new ValueTask<T>(map(x)))
        { }
        
        private MappedCachedObject(ICachedObject<TSource> source, Func<TSource, ValueTask<T>> map)
        {
            _source = source;
            _map = map;
            _sourceUpdateFuncInputType = GetSourceUpdateFuncInputType();
            
            _source.OnValueRefreshed += OnSourceValueRefreshed;
            SubscribeToSourceUpdatedEvents();
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

        public override void Dispose()
        {
            _source.OnValueRefreshed -= OnSourceValueRefreshed;
            UnsubscribeFromSourceUpdatedEvents();
            base.Dispose();
        }

        private async void OnSourceValueRefreshed(object _, ValueRefreshedEvent<TSource> args)
        {
            await UpdateValue(args.NewValue, args.Version).ConfigureAwait(false);
        }
        
        private async void OnSourceValueUpdated<TUpdateFuncInput>(object _, ValueUpdatedEvent<TSource, TUpdateFuncInput> args)
        {
            await UpdateValue(args.NewValue, args.Version).ConfigureAwait(false);
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

        private void SubscribeToSourceUpdatedEvents()
        {
            if (_sourceUpdateFuncInputType is null)
                return;
            
            var method = this.GetType()
                .GetMethod(nameof(SubscribeToSourceUpdatedEventsImpl), BindingFlags.Instance | BindingFlags.NonPublic)
                ?.MakeGenericMethod(_sourceUpdateFuncInputType);

            method?.Invoke(this, null);
        }
        
        private void SubscribeToSourceUpdatedEventsImpl<TUpdateFuncInput>()
        {
            ((ICachedObjectWithUpdates<TSource, TUpdateFuncInput>)_source).OnValueUpdated += OnSourceValueUpdated;
        }
        
        private void UnsubscribeFromSourceUpdatedEvents()
        {
            if (_sourceUpdateFuncInputType is null)
                return;
            
            var method = this.GetType()
                .GetMethod(nameof(UnsubscribeToSourceUpdatedEventsImpl), BindingFlags.Instance | BindingFlags.NonPublic)
                ?.MakeGenericMethod(_sourceUpdateFuncInputType);

            method?.Invoke(this, null);
        }
        
        private void UnsubscribeToSourceUpdatedEventsImpl<TUpdateFuncInput>()
        {
            ((ICachedObjectWithUpdates<TSource, TUpdateFuncInput>)_source).OnValueUpdated -= OnSourceValueUpdated;
        }

        private Type GetSourceUpdateFuncInputType()
        {
            return _source
                .GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType)
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(ICachedObjectWithUpdates<,>))
                ?.GenericTypeArguments[1];
        }
    }
}