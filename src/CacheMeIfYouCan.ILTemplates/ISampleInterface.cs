using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.ILTemplates
{
    public interface ISampleInterface
    {
        Task<string> SingleKeyAsync(string key);
        string SingleKeySync(string key);
        Task<string> SingleKeyAsyncCanx(string key, CancellationToken cancellationToken);
        string SingleKeySyncCanx(string key, CancellationToken cancellationToken);
        Task<Dictionary<string, string>> EnumerableKeyAsync(IEnumerable<string> keys);
        Dictionary<string, string> EnumerableKeySync(IEnumerable<string> keys);
        Task<Dictionary<string, string>> EnumerableKeyAsyncCanx(IEnumerable<string> keys, CancellationToken cancellationToken);
        Dictionary<string, string> EnumerableKeySyncCanx(IEnumerable<string> keys, CancellationToken cancellationToken);
    }
}