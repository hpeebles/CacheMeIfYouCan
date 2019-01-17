using System;
using CacheMeIfYouCan.Configuration;
using NCrontab;

namespace CacheMeIfYouCan.Cron
{
    public static class CachedObjectConfigurationManagerExtensions
    {
        public static CachedObjectConfigurationManager<T> WithRefreshSchedule<T>(
            this CachedObjectConfigurationManager<T> config,
            string cronExpression,
            bool includingSeconds = false)
        {
            var options = new CrontabSchedule.ParseOptions();
            if (includingSeconds)
                options.IncludingSeconds = true;
            
            var schedule = CrontabSchedule.Parse(cronExpression, options);

            return config
                .WithJitterPercentage(0)
                .WithRefreshInterval(GetNextInterval);
            
            TimeSpan GetNextInterval()
            {
                var now = DateTime.UtcNow;
                
                return schedule.GetNextOccurrence(now) - now;
            }
        }
    }
}