using System;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class MockTelemetry
    {
        public string Host { get; set; }
        public string Cache { get; set; }
        public string Command { get; set; }
        public DateTimeOffset Start { get; set; }
        public TimeSpan Duration { get; set; }
    }
}