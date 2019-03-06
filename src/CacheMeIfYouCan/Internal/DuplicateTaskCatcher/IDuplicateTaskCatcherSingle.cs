using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal interface IDuplicateTaskCatcherSingle<in TK, TV>
    {
        Task<(ValueWithTimestamp<TV> value, bool duplicate)> ExecuteAsync(TK key, CancellationToken token);
    }
}