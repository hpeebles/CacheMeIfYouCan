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
    }
}