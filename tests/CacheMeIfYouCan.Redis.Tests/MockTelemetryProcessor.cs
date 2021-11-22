using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class MockTelemetryProcessor : ITelemetryProcessor
    {
        private readonly List<MockTelemetry> _telemetry = new List<MockTelemetry>();
        
        public void Process(ITelemetry item)
        {
            var data = item as DependencyTelemetry;
            _telemetry.Add(new 
                MockTelemetry
                {
                    Host = data?.Target,
                    Cache = data?.Name,
                    Command = data?.Data,
                    Start = data?.Timestamp ?? DateTimeOffset.MinValue,
                    Duration = data?.Duration ?? TimeSpan.Zero
                });
        }

        public List<MockTelemetry> GetTrace()
        {
            return _telemetry;
        }
    }
}