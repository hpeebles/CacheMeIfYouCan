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

        public string StringToStringSync(string key)
        {
            return key;
        }
    }
}