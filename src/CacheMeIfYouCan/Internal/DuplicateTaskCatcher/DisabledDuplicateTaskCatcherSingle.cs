using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal class DisabledDuplicateTaskCatcherSingle<TK, TV> : IDuplicateTaskCatcherSingle<TK, TV>
    {
        private readonly Func<TK, CancellationToken, Task<TV>> _func;

        public DisabledDuplicateTaskCatcherSingle(Func<TK, CancellationToken, Task<TV>> func)
        {
            _func = func;
        }
        
        public async Task<(ValueWithTimestamp<TV> value, bool duplicate)> ExecuteAsync(TK key, CancellationToken token)
        {
            var result = await _func(key, token);

            var returnValue = new ValueWithTimestamp<TV>(result, Stopwatch.GetTimestamp());

            return (returnValue, false);
        }
    }
}