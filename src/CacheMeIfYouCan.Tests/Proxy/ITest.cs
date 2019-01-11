using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public interface ITest
    {
        Task<string> StringToString(string key);

        Task<string> IntToString(int key);

        Task<int> LongToInt(long key);

        Task<IDictionary<string, string>> MultiEcho(IEnumerable<string> keys);
        
        Task<IDictionary<string, string>> MultiEchoList(IList<string> keys);
        
        Task<IDictionary<string, string>> MultiEchoSet(ISet<string> keys);

        string StringToStringSync(string key);
        
        IDictionary<string, string> MultiStringToStringSync(ICollection<string> keys);

        Task<ConcurrentDictionary<string, string>> MultiEchoToConcurrent(IEnumerable<string> keys);
    }
}