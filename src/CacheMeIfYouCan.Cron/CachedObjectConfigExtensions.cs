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
                return schedule.GetNextOccurrence(DateTime.UtcNow) - DateTime.UtcNow;
            }

            return config
                .WithJitterPercentage(0)
                .WithRefreshInterval(GetNextInterval);
        }
    }
}