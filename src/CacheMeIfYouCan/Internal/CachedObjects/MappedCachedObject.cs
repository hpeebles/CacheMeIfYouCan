using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal.CachedObjects
{
    internal sealed class MappedCachedObject<TSource, TSourceUpdates, T> : UpdateableCachedObjectBase<T, TSourceUpdates>
    {
        private readonly ICachedObject<TSource> _source;
        private readonly Func<T, TSource, TSourceUpdates, Task<T>> _mapUpdatesFunc;
        private long _sourceVersion;

        public MappedCachedObject(
            ICachedObject<TSource> source,
            Func<TSource, Task<T>> map,
            Func<T, TSource, TSourceUpdates, Task<T>> mapUpdatesFunc = null)
            : base(
                cancellationToken => GetValue(source, map, cancellationToken),
                null,
                null)
        {
            _source = source;
            _mapUpdatesFunc = mapUpdatesFunc;

            _source.OnValueRefreshed += OnSourceValueRefreshed;
            SubscribeToSourceUpdatedEvents();

            _source.OnDisposed += (_, __) => Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _source.OnValueRefreshed -= OnSourceValueRefreshed;
            UnsubscribeFromSourceUpdatedEvents();
        }

        private async void OnSourceValueRefreshed(object _, ValueRefreshedEvent<TSource> args)
        {
            try
            {
                await RefreshValue(args.Version);
            }
            catch
            {
                // What should happen here??
            }
        }
        
        private async void OnSourceValueUpdated(object _, ValueUpdatedEvent<TSource, TSourceUpdates> args)
        {
            try
            {
                var task = _mapUpdatesFunc is null
                    ? RefreshValue(args.Version)
                    : UpdateValue(args.NewValue, args.Updates, args.Version);

                await task.ConfigureAwait(false);
            }
            catch
            {
                // What should happen here??
            }
        }

        private async Task RefreshValue(long sourceVersion)
        {
            if (State != CachedObjectState.Ready)
                return;

            if (sourceVersion <= _sourceVersion)
                return;
            
            await RefreshValueAsync().ConfigureAwait(false);
                
            _sourceVersion = sourceVersion;
        }

        private async Task UpdateValue(TSource sourceValue, TSourceUpdates sourceUpdates, long sourceVersion)
        {
            if (State != CachedObjectState.Ready)
                return;

            try
            {
                await AcquireUpdateValueLock(_isDisposedCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (sourceVersion <= _sourceVersion)
                return;

            await UpdateValueWithinLock(MapUpdatesFunc, sourceUpdates, CancellationToken.None);

            _sourceVersion = sourceVersion;

            Task<T> MapUpdatesFunc(T value, TSourceUpdates updates, CancellationToken _)
            {
                return _mapUpdatesFunc(value, sourceValue, updates);
            }
        }

        private void SubscribeToSourceUpdatedEvents()
        {
            if (_source is IIncrementalCachedObject<TSource, TSourceUpdates> i)
                i.OnValueUpdated += OnSourceValueUpdated;
            else if (_source is IUpdateableCachedObject<TSource, TSourceUpdates> u)
                u.OnValueUpdated += OnSourceValueUpdated;
        }
        
        private void UnsubscribeFromSourceUpdatedEvents()
        {
            if (_source is IIncrementalCachedObject<TSource, TSourceUpdates> i)
                i.OnValueUpdated -= OnSourceValueUpdated;
            else if (_source is IUpdateableCachedObject<TSource, TSourceUpdates> u)
                u.OnValueUpdated -= OnSourceValueUpdated;
        }
        
        private static async Task<T> GetValue(
            ICachedObject<TSource> source,
            Func<TSource, Task<T>> map,
            CancellationToken cancellationToken)
        {
            if (source.State != CachedObjectState.Ready)
                await source.InitializeAsync(cancellationToken).ConfigureAwait(false);
            
            return await map(source.Value).ConfigureAwait(false);
        }
    }
}