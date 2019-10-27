using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace CacheMeIfYouCan.ApplicationInsights
{
    internal class LocalCacheApplicationInsightsWrapper<TK, TV> : ILocalCache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;
        private readonly string _host;
        private readonly Trimmer _trimmer;
        private readonly TelemetryClient _telemetryClient;

        public LocalCacheApplicationInsightsWrapper(ILocalCache<TK, TV> cache, CacheApplicationInsightsConfig config)
        {
            _cache = cache;
            _host = config.Host;
            _trimmer = new Trimmer(config.KeyCountLimit ?? Int32.MaxValue);
            _telemetryClient = new TelemetryClient();
        }

        public string CacheName => _cache.CacheName;
        public string CacheType => _cache.CacheType;
        public bool RequiresKeySerializer => _cache.RequiresKeySerializer;
        public bool RequiresKeyComparer => _cache.RequiresKeyComparer;
        public void Dispose() => _cache.Dispose();

        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Get 1 key");
            builder.AppendLine(key.AsStringSafe);

            return Execute(() => _cache.Get(key), builder.ToString());
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Set 1 key. TTL {timeToLive}");
            builder.AppendLine(key.AsStringSafe);

            Execute(() => _cache.Set(key, value, timeToLive), builder.ToString());
        }

        public IList<GetFromCacheResult<TK, TV>> Get(IReadOnlyCollection<Key<TK>> keys)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Get {keys.Count} key(s)");
            builder.AppendLine(String.Join(Environment.NewLine, _trimmer.Trim(keys).Select(k => k.AsStringSafe)));

            return Execute(() => _cache.Get(keys), builder.ToString());
        }

        public void Set(IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Set {values.Count} key(s). TTL {timeToLive}");
            builder.AppendLine(String.Join(Environment.NewLine, _trimmer.Trim(values).Select(kv => kv.Key.AsStringSafe)));

            Execute(() => _cache.Set(values, timeToLive), builder.ToString());
        }

        public bool Remove(Key<TK> key)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Remove 1 key");
            builder.AppendLine(key.AsStringSafe);

            return Execute(() => _cache.Remove(key), builder.ToString());
        }

        private T Execute<T>(Func<T> func, string commandInfoText)
        {
            using (_telemetryClient.StartOperation(new DependencyTelemetry(CacheType, _host, CacheName, commandInfoText)))
                return func();
        }

        private void Execute(Action func, string commandInfoText)
        {
            using (_telemetryClient.StartOperation(new DependencyTelemetry(CacheType, _host, CacheName, commandInfoText)))
                func();
        }
    }
}
