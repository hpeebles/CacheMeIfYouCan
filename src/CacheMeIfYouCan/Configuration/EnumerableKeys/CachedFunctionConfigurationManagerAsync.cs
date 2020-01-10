using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.EnumerableKeys
{
    public sealed class CachedFunctionConfigurationManagerAsync<TKey, TValue, TRequest, TResponse>
        : CachedFunctionConfigurationManagerBase<TKey, TValue, TRequest, TResponse, CachedFunctionConfigurationManagerAsync<TKey, TValue, TRequest, TResponse>>
        where TRequest : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TRequest, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsync(Func<TRequest, Task<TResponse>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TRequest, Task<TResponse>> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async Task<TResponse> Get(TRequest request)
            {
                var task = cachedFunction.GetMany(request, CancellationToken.None);

                if (!task.IsCompleted)
                    await task.ConfigureAwait(false);

                var results = task.Result;

                return results switch
                {
                    null => default,
                    TResponse typedResponse => typedResponse,
                    _ => responseConverter(task.Result)
                };
            }
        }

        private Func<IReadOnlyCollection<TKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            async Task<IEnumerable<KeyValuePair<TKey, TValue>>> Get(
                IReadOnlyCollection<TKey> keys,
                CancellationToken cancellationToken)
            {
                if (!(keys is TRequest typedRequest))
                    typedRequest = requestConverter(keys);

                var task = _originalFunction(typedRequest);

                if (task.IsCompleted)
                    await task.ConfigureAwait(false);

                return task.Result;
            }
        }
    }
}