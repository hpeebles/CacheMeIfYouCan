namespace CacheMeIfYouCan.Redis
{
    public interface IRedisTelemetryConfig
    {
        public int MillisecondThreshold { get; }
    }
}