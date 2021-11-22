using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisTelemetry : IRedisTelemetry
    {
        private readonly ITelemetryProcessor _telemetryProcessor;
        private readonly string _host;
        private readonly string _cacheName;

        public RedisTelemetry(ITelemetryProcessor telemetryProcessor, IRedisTelemetryConfig redisTelemetryConfig, string host, string cacheName)
        {
            _telemetryProcessor = new RedisTelemetryProcessor(telemetryProcessor, redisTelemetryConfig);
            _host = host;
            _cacheName = cacheName;
        }

        public async Task<T> CallRedisAsync<T>(Func<Task<T>> func, string command, string key)
        {
            var success = false;
            var commandInfoText = $"Command {command}{Environment.NewLine}Key '{key}'";

            T result;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // making dependency call
                result = await func().ConfigureAwait(false);
                success = true;
            }
            finally
            {
                timer.Stop();
                _telemetryProcessor.Process(
                    new DependencyTelemetry("Redis", _host, _cacheName, commandInfoText, startTime, timer.Elapsed, "",
                        success));
            }
            return result;
        }
    }

    internal class NoTelemetry : IRedisTelemetry
    {
        public async Task<T> CallRedisAsync<T>(Func<Task<T>> func, string command, string key)
        {
            return await func().ConfigureAwait(false);
        }
    }
}