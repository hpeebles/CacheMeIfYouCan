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
    }
}