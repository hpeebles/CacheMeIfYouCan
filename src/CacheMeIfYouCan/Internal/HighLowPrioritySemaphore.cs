using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class HighLowPrioritySemaphore
    {
        private readonly Queue<TaskCompletionSource<bool>> _lowPriorityQueue = new Queue<TaskCompletionSource<bool>>();
        private readonly Queue<TaskCompletionSource<bool>> _highPriorityQueue = new Queue<TaskCompletionSource<bool>>();
        private readonly object _lock = new Object();
        private readonly int _maxCount;
        private int _currentCount;

        public HighLowPrioritySemaphore(int initialCount, int maxCount)
        {
            _currentCount = initialCount;
            _maxCount = maxCount;
        }

        public bool TryAcquireWithoutWaiting()
        {
            lock (_lock)
            {
                if (_currentCount == 0)
                    return false;

                _currentCount--;
                return true;
            }
        }
        
        public Task<bool> WaitAsync(bool highPriority, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                if (_currentCount > 0)
                {
                    _currentCount--;
                    return Task.FromResult(true);
                }

                var tcs = new TaskCompletionSource<bool>();
                if (highPriority)
                    _highPriorityQueue.Enqueue(tcs);
                else
                    _lowPriorityQueue.Enqueue(tcs);

                return cancellationToken.CanBeCanceled
                    ? WaitWithCancellationAsync(tcs, cancellationToken)
                    : tcs.Task;
            }
        }

        public void Release()
        {
            lock (_lock)
            {
                while (_highPriorityQueue.Count > 0)
                {
                    var next = _highPriorityQueue.Dequeue();
                    if (next.TrySetResult(true))
                        return;
                }

                while (_lowPriorityQueue.Count > 0)
                {
                    var next = _lowPriorityQueue.Dequeue();
                    if (next.TrySetResult(true))
                        return;
                }

                if (_currentCount < _maxCount)
                    _currentCount++;
            }
        }

        private static async Task<bool> WaitWithCancellationAsync(
            TaskCompletionSource<bool> tcs,
            CancellationToken cancellationToken)
        {
            using var _ = cancellationToken.Register(() => tcs.TrySetResult(false));

            return await tcs.Task.ConfigureAwait(false);
        }
    }
}