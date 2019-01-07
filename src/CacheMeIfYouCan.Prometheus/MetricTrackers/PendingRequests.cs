using System;
using System.Reactive.Linq;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus.MetricTrackers
{
    internal static class PendingRequests
    {
        private static readonly Gauge PendingRequestsCounter;
        
        static PendingRequests()
        {
            PendingRequestsCounter = Metrics.CreateGauge("PendingRequestsCounter", null, "name", "type");

            Observable
                .Interval(TimeSpan.FromSeconds(15))
                .Do(_ => TrackPendingRequestCounts())
                .Retry()
                .Subscribe();
        }

        private static void TrackPendingRequestCounts()
        {
            foreach (var count in PendingRequestsCounterContainer.GetCounts())
            {
                PendingRequestsCounter
                    .Labels(count.Name, count.Type)
                    .Set(count.Count);
            }
        }
    }
}