using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using NCrontab;

namespace CacheMeIfYouCan.Cron
{
    public static class CachedObjectConfigurationManagerCronExtensions
    {
        public static ICachedObjectConfigurationManager<T> WithRefreshSchedule<T>(
            this ICachedObjectConfigurationManager<T> config,
            string cronExpression,
            bool includingSeconds = false)
        {
            var options = new CrontabSchedule.ParseOptions();
            if (includingSeconds)
                options.IncludingSeconds = true;
            
            var schedule = CrontabSchedule.Parse(cronExpression, options);
            
            var scheduler = new CachedObjectRefreshScheduler<T>(schedule);

            config.OnInitialized(c => scheduler.Start(c));
            config.OnDisposed(c => scheduler.Dispose());
            
            return config;
        }

        private class CachedObjectRefreshScheduler<T> : IDisposable
        {
            private readonly CrontabSchedule _schedule;
            private ICachedObject<T> _cachedObject;
            private Timer _timer;
            private Action _updateTimer;

            public CachedObjectRefreshScheduler(CrontabSchedule schedule)
            {
                _schedule = schedule;
            }

            public void Start(ICachedObject<T> cachedObject)
            {
                _cachedObject = cachedObject;
                _timer = new Timer(async _ => await RunUpdate());
                _updateTimer = UpdateTimer;
                _updateTimer();
            }

            public void Dispose() => _timer.Dispose();

            private async Task RunUpdate()
            {
                try
                {
                    await _cachedObject.RefreshValueAsync().ConfigureAwait(false);
                }
                finally
                {
                    _updateTimer.Invoke();
                }
            }
            
            private void UpdateTimer()
            {
                var now = DateTime.UtcNow;
                var interval = _schedule.GetNextOccurrence(now) - now;

                _timer.Change((long)interval.TotalMilliseconds, -1);
            }
        }
    }
}