using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal interface IDuplicateTaskCatcherMulti<TK, TV>
    {
        Task<IDictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>> ExecuteAsync(
            IReadOnlyCollection<TK> keys,
            CancellationToken token = default);
    }
}