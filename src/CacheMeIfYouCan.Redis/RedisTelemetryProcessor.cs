using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryConfig _telemetryConfig;
        private ITelemetryProcessor Next { get; }

        // next will point to the next RedisTelemetryProcessor in the chain.
        public RedisTelemetryProcessor(ITelemetryProcessor next, ITelemetryConfig telemetryConfig)
        {
            _telemetryConfig = telemetryConfig;
            Next = next;
        }

        public void Process(ITelemetry item)
        {
            if (item is DependencyTelemetry request 
                && request.Duration.TotalMilliseconds > _telemetryConfig?.MillisecondThreshold)
            {
                Next.Process(item);
            }
        }
    }
}
