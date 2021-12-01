namespace CacheMeIfYouCan
{
    /// <summary>
    /// Settings used to control what telemetry is collected
    /// </summary>
    public interface ITelemetryConfig
    {
        int MillisecondThreshold { get; }
    }
}
