namespace CacheMeIfYouCan.Redis.Tests
{
    public class TelemetryConfig : ITelemetryConfig
    {
        public int MillisecondThreshold { get; set; }
    }
}
