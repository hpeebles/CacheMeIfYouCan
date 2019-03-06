using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal interface IDuplicateTaskCatcherCombinedMulti<in TK1, TK2, TV>
    {
        Task<IDictionary<TK2, DuplicateTaskCatcherMultiResult<TK2, TV>>> ExecuteAsync(
            TK1 outerKey,
            ICollection<TK2> innerKeys,
            CancellationToken token);
    }
}