using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CacheMeIfYouCan.Redis
{
    internal class TelemetryProcessor
    {
        private readonly ITelemetryProcessor _telemetryProcessor;
        private readonly IDistributedCacheConfig _config;

        public TelemetryProcessor(IDistributedCacheConfig config, 
            ITelemetryProcessor telemetryProcessor,
            ITelemetryConfig telemetryConfig)
        {
            _telemetryProcessor = new RedisTelemetryProcessor(telemetryProcessor, telemetryConfig);
            _config = config;
        }

        public void Add(TimeSpan duration, string command,
            string keyOrKeys, bool successful)
        {
            _telemetryProcessor.Process(
                new DependencyTelemetry(
                    _config.CacheType,
                    _config.Host,
                    _config.CacheName,
                    $"Command {command}{Environment.NewLine}{keyOrKeys}",
                    DateTime.UtcNow - duration,
                    duration,
                    "", successful));
        }
    }
}