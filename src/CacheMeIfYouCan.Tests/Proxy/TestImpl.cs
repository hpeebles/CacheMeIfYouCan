using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class TestImpl : ITest
    {
        public Task<string> StringToString(string key)
        {
            return Task.FromResult(key);
        }

        public Task<string> IntToString(int key)
        {
            return Task.FromResult(key.ToString());
        }

        public Task<int> LongToInt(long key)
        {
            return Task.FromResult((int) key * 2);
        }

        public Task<IDictionary<string, string>> MultiEcho(IEnumerable<string> keys)
        {
            return Task.FromResult<IDictionary<string, string>>(keys.ToDictionary(k => k));
        }
        
        public Task<IDictionary<string, string>> MultiEchoList(IList<string> keys)
        {
            return Task.FromResult<IDictionary<string, string>>(keys.ToDictionary(k => k));
        }
        
        public Task<IDictionary<string, string>> MultiEchoSet(ISet<string> keys)
        {
            return Task.FromResult<IDictionary<string, string>>(keys.ToDictionary(k => k));
        }

        public string StringToStringSync(string key)
        {
            return key;
        }

        public IDictionary<string, string> MultiStringToStringSync(ICollection<string> keys)
        {
            return keys.ToDictionary(k => k);
        }
        
        public Task<ConcurrentDictionary<string, string>> MultiEchoToConcurrent(IEnumerable<string> keys)
        {
            return Task.FromResult(new ConcurrentDictionary<string, string>(keys.ToDictionary(k => k)));
        }

        public Task<string> MultiParamEcho(string key1, int key2)
        {
            return Task.FromResult($"{key1}_{key2}");
        }
        
        public string MultiParamEchoSync(string key1, int key2)
        {
            return $"{key1}_{key2}";
        }
    }
}