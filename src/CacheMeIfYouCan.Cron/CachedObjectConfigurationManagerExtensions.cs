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
            var scheduler = BuildScheduler(cronExpression, includingSeconds);

            config.OnInitialized(c => scheduler.Start(() => c.RefreshValueAsync()));
            config.OnDisposed(c => scheduler.Dispose());
            
            return config;
        }
        
        public static IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithRefreshSchedule<T, TUpdates>(
            this IIncrementalCachedObjectConfigurationManager<T, TUpdates> config,
            string cronExpression,
            bool includingSeconds = false)
        {
            var scheduler = BuildScheduler(cronExpression, includingSeconds);

            config.OnInitialized(c => scheduler.Start(() => c.RefreshValueAsync()));
            config.OnDisposed(c => scheduler.Dispose());
            
            return config;
        }

        public static IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithRefreshSchedule<T, TUpdates>(
            this IUpdateableCachedObjectConfigurationManager<T, TUpdates> config,
            string cronExpression,
            bool includingSeconds = false)
        {
            var scheduler = BuildScheduler(cronExpression, includingSeconds);

            config.OnInitialized(c => scheduler.Start(() => c.RefreshValueAsync()));
            config.OnDisposed(c => scheduler.Dispose());
            
            return config;
        }
        
        public static IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdateSchedule<T, TUpdates>(
            this IIncrementalCachedObjectConfigurationManager<T, TUpdates> config,
            string cronExpression,
            bool includingSeconds = false)
        {
            var scheduler = BuildScheduler(cronExpression, includingSeconds);

            config.OnInitialized(c => scheduler.Start(() => c.UpdateValueAsync()));
            config.OnDisposed(c => scheduler.Dispose());
            
            return config;
        }

        private static Scheduler BuildScheduler(string cronExpression, bool includingSeconds)
        {
            var options = new CrontabSchedule.ParseOptions();
            if (includingSeconds)
                options.IncludingSeconds = true;
            
            var schedule = CrontabSchedule.Parse(cronExpression, options);
            
            return new Scheduler(schedule);
        }

        private sealed class Scheduler : IDisposable
        {
            private readonly CrontabSchedule _schedule;
            private Func<Task> _job;
            private Timer _timer;

            public Scheduler(CrontabSchedule schedule)
            {
                _schedule = schedule;
            }

            public void Start(Func<Task> job)
            {
                _job = job;
                _timer = new Timer(async _ => await RunJob().ConfigureAwait(false));
                UpdateTimer();
            }

            public void Dispose() => _timer.Dispose();

            private async Task RunJob()
            {
                try
                {
                    await _job().ConfigureAwait(false);
                }
                finally
                {
                    UpdateTimer();
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