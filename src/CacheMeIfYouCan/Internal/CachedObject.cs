using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObject<T> : ICachedObject<T>, IDisposable
    {
        private readonly Func<Task<T>> _getValueFunc;
        private readonly Func<TimeSpan> _intervalFunc;
        private readonly Action<Exception> _onError;
        private readonly Queue<RefreshValueResult> _previousRefreshValueResults;
        private readonly object _lock = new Object();
        private T _value;
        private bool _initialised;
        private IDisposable _refreshTask;

        public CachedObject(Func<Task<T>> getValueFunc, Func<TimeSpan> intervalFunc, Action<Exception> onError)
        {
            _getValueFunc = getValueFunc ?? throw new ArgumentNullException(nameof(getValueFunc));
            _intervalFunc = intervalFunc ?? throw new ArgumentNullException(nameof(intervalFunc));
            _onError = onError;
            _previousRefreshValueResults = new Queue<RefreshValueResult>();
        }

        public T Value
        {
            get
            {
                if (!_initialised)
                    Init().GetAwaiter().GetResult();
                
                return _value;
            }
        }

        public async Task<bool> Init()
        {
            if (_initialised)
                return true;

            var lockTaken = false;
            Monitor.TryEnter(_lock, ref lockTaken);

            if (_initialised)
            {
                if (lockTaken)
                    Monitor.Exit(_lock);

                return true;
            }

            try
            {
                var result = await RefreshValue();

                LogRefreshValueResult(result);

                if (!result.Success)
                    return false;

                _refreshTask = Observable
                    .Defer(() => RefreshValue()
                        .ToObservable()
                        .DelaySubscription(_intervalFunc()))
                    .Repeat()
                    .Subscribe(LogRefreshValueResult);
                    
                _initialised = true;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_lock);
            }

            return _initialised;
        }

        public void Dispose()
        {
            _refreshTask?.Dispose();
        }

        private async Task<RefreshValueResult> RefreshValue()
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            Exception exception = null;

            try
            {
                _value = await _getValueFunc();
            }
            catch (Exception ex)
            {
                exception = ex;
                
                _onError?.Invoke(ex);
            }
            
            var duration = StopwatchHelper.GetDuration(stopwatchStart);

            return new RefreshValueResult
            {
                Start = start,
                Duration = TimeSpan.FromTicks(duration),
                Success = exception == null,
                Exception = exception
            };
        }

        private void LogRefreshValueResult(RefreshValueResult result)
        {
            while (_previousRefreshValueResults.Count >= 10)
                _previousRefreshValueResults.Dequeue();
                        
            _previousRefreshValueResults.Enqueue(result);
        }
        
        private class RefreshValueResult
        {
            public DateTime Start;
            public TimeSpan Duration;
            public bool Success;
            public Exception Exception;
        }
    }
}