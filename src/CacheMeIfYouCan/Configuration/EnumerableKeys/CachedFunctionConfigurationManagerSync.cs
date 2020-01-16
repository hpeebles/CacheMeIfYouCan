using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.EnumerableKeys
{
    public sealed class CachedFunctionConfigurationManagerSync<TRequest, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerBase<TRequest, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync<TRequest, TResponse, TKey, TValue>>
        where TRequest : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TRequest, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync(Func<TRequest, TResponse> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TRequest, TResponse> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            TResponse Get(TRequest request)
            {
                var task = cachedFunction.GetMany(request, CancellationToken.None);

                if (!task.IsCompleted)
                    Task.Run(() => task).GetAwaiter().GetResult();

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
            
            Task<IEnumerable<KeyValuePair<TKey, TValue>>> Get(
                IReadOnlyCollection<TKey> keys,
                CancellationToken cancellationToken)
            {
                if (!(keys is TRequest typedRequest))
                    typedRequest = requestConverter(keys);

                return Task.FromResult((IEnumerable<KeyValuePair<TKey, TValue>>)_originalFunction(typedRequest));
            }
        }
    }
}