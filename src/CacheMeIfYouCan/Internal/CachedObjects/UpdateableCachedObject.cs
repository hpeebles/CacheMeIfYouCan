using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.CachedObjects
{
    internal sealed class UpdateableCachedObject<T, TUpdates> : UpdateableCachedObjectBase<T, TUpdates>, IUpdateableCachedObject<T, TUpdates>
    {
        private readonly Func<T, TUpdates, CancellationToken, Task<T>> _applyUpdatesFunc;
        private readonly Channel<(TUpdates Updates, TaskCompletionSource<bool> Tcs, CancellationToken CancellationToken)> _updatesQueue;

        public UpdateableCachedObject(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc,
            Func<TimeSpan> refreshIntervalFactory,
            TimeSpan? refreshValueFuncTimeout)
            : base(getValueFunc, refreshIntervalFactory, refreshValueFuncTimeout)
        {
            _applyUpdatesFunc = applyUpdatesFunc;
            
            var options = new UnboundedChannelOptions { SingleReader = true };
            _updatesQueue = Channel.CreateUnbounded<(TUpdates, TaskCompletionSource<bool>, CancellationToken)>(options);
            
            Task.Run(ProcessUpdates);
        }

        public void UpdateValue(TUpdates updates)
        {
            Task.Run(() => UpdateValueAsync(updates)).GetAwaiter().GetResult();
        }
        
        public async Task UpdateValueAsync(TUpdates updates, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (State != CachedObjectState.Ready)
                await InitializeAsync(cancellationToken).ConfigureAwait(false);

            if (TryAcquireRefreshOrUpdateValueLockWithoutWaiting())
            {
                await UpdateValueWithinLock(_applyUpdatesFunc, updates, cancellationToken).ConfigureAwait(false);
                return;
            }

            var tcs = new TaskCompletionSource<bool>();

            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => tcs.SetCanceled());

            _updatesQueue.Writer.TryWrite((updates, tcs, cancellationToken));

            await tcs.Task.ConfigureAwait(false);
        }
        
        private async void ProcessUpdates()
        {
            if (_isDisposedCancellationTokenSource.IsCancellationRequested)
                return;
            
            try
            {
                while (await _updatesQueue
                    .Reader
                    .WaitToReadAsync(_isDisposedCancellationTokenSource.Token)
                    .ConfigureAwait(false))
                {
                    while (_updatesQueue.Reader.TryRead(out var next))
                    {
                        if (_isDisposedCancellationTokenSource.IsCancellationRequested)
                            return;
                        
                        if (next.CancellationToken.IsCancellationRequested)
                            continue;

                        try
                        {
                            if (!TryAcquireRefreshOrUpdateValueLockWithoutWaiting())
                            {
                                using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                                    _isDisposedCancellationTokenSource.Token,
                                    next.CancellationToken);

                                try
                                {
                                    await AcquireUpdateValueLock(cts.Token).ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    if (_isDisposedCancellationTokenSource.IsCancellationRequested)
                                        return;

                                    continue;
                                }
                            }

                            await UpdateValueWithinLock(_applyUpdatesFunc, next.Updates, next.CancellationToken).ConfigureAwait(false);
                            next.Tcs.TrySetResult(true);
                        }
                        catch (Exception ex)
                        {
                            next.Tcs.TrySetException(ex);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (ObjectDisposedException)
            { }
        }
    }
}