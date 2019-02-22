using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace CacheMeIfYouCan.ApplicationInsights
{
    internal class DistributedCacheApplicationInsightsWrapper<TK, TV> : IDistributedCache<TK, TV>
    {
        private readonly IDistributedCache<TK, TV> _cache;
        private readonly string _host;
        private readonly Trimmer _trimmer;
        private readonly TelemetryClient _telemetryClient;

        public DistributedCacheApplicationInsightsWrapper(IDistributedCache<TK, TV> cache, CacheApplicationInsightsConfig config)
        {
            _cache = cache;
            _host = config.Host;
            _trimmer = new Trimmer(config.KeyCountLimit ?? Int32.MaxValue);
            _telemetryClient = new TelemetryClient();
        }

        public string CacheName => _cache.CacheName;
        public string CacheType => _cache.CacheType;
        public void Dispose() => _cache.Dispose();

        public Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Get 1 key");
            builder.AppendLine(key.AsStringSafe);

            return Execute(() => _cache.Get(key), builder.ToString());
        }

        public Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Set 1 key. TTL {timeToLive}");
            builder.AppendLine(key.AsStringSafe);

            return Execute(() => _cache.Set(key, value, timeToLive), builder.ToString());
        }

        public Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Get {keys.Count} key(s)");
            builder.AppendLine(String.Join(Environment.NewLine, _trimmer.Trim(keys).Select(k => k.AsStringSafe)));

            return Execute(() => _cache.Get(keys), builder.ToString());
        }

        public Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Set {values.Count} key(s). TTL {timeToLive}");
            builder.AppendLine(String.Join(Environment.NewLine, _trimmer.Trim(values).Select(kv => kv.Key.AsStringSafe)));

            return Execute(() => _cache.Set(values, timeToLive), builder.ToString());
        }

        public Task<bool> Remove(Key<TK> key)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Remove 1 key");
            builder.AppendLine(key.AsStringSafe);

            return Execute(() => _cache.Remove(key), builder.ToString());
        }

        private async Task<T> Execute<T>(Func<Task<T>> func, string commandInfoText)
        {
            using (_telemetryClient.StartOperation(new DependencyTelemetry(CacheType, _host, CacheName, commandInfoText)))
                return await func();
        }

        private async Task Execute(Func<Task> func, string commandInfoText)
        {
            using (_telemetryClient.StartOperation(new DependencyTelemetry(CacheType, _host, CacheName, commandInfoText)))
                await func();
        }
    }
}
