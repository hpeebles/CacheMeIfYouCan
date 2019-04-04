using System;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Configuration.CachedObject;
using CacheMeIfYouCan.Notifications;
using NCrontab;

namespace CacheMeIfYouCan.Cron
{
    public static class CachedObjectConfigurationManagerExtensions
    {
        public static CachedObjectConfigurationManager<T, Unit> WithRefreshSchedule<T>(
            this CachedObjectConfigurationManager_ConfigureFor<T> config,
            string cronExpression,
            bool includingSeconds = false)
        {
            var options = new CrontabSchedule.ParseOptions();
            if (includingSeconds)
                options.IncludingSeconds = true;
            
            var schedule = CrontabSchedule.Parse(cronExpression, options);

            return config.WithRefreshIntervalFactory(GetNextInterval);
            
            TimeSpan GetNextInterval(CachedObjectUpdateResult<T, Unit> result)
            {
                var now = DateTime.UtcNow;
                
                return schedule.GetNextOccurrence(now) - now;
            }
        }
    }
}