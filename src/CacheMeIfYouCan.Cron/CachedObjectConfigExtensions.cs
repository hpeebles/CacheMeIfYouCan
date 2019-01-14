using System;
using CacheMeIfYouCan.Configuration;
using NCrontab;

namespace CacheMeIfYouCan.Cron
{
    public static class CachedObjectConfigExtensions
    {
        public static CachedObjectConfigManager<T> WithRefreshSchedule<T>(
            this CachedObjectConfigManager<T> config,
            string cronExpression,
            bool includingSeconds = false)
        {
            var options = new CrontabSchedule.ParseOptions();
            if (includingSeconds)
                options.IncludingSeconds = true;
            
            var schedule = CrontabSchedule.Parse(cronExpression, options);

            TimeSpan GetNextInterval()
            {
                var now = DateTime.UtcNow;
                
                return schedule.GetNextOccurrence(now) - now;
            }

            return config
                .WithJitterPercentage(0)
                .WithRefreshInterval(GetNextInterval);
        }
    }
}