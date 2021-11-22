namespace CacheMeIfYouCan.Redis.Tests
{
    public class MockTelemetryConfig : ITelemetryConfig
    {
        public MockTelemetryConfig(int millisecondThreshold)
        {
            MillisecondThreshold = millisecondThreshold;
        }

        public int MillisecondThreshold { get; }
    }
}