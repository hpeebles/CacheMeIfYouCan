using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public sealed class CachedFunctionConfigurationManagerAsync<TOuterKey, TInnerKey, TValue, TInnerRequest, TResponse>
        : CachedFunctionConfigurationManagerBase<TOuterKey, TInnerKey, TValue, TInnerRequest, TResponse, CachedFunctionConfigurationManagerAsync<TOuterKey, TInnerKey, TValue, TInnerRequest, TResponse>>
        where TInnerRequest : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        private readonly Func<TOuterKey, TInnerRequest, Task<TResponse>> _originalFunction;

        public CachedFunctionConfigurationManagerAsync(Func<TOuterKey, TInnerRequest, Task<TResponse>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TOuterKey, TInnerRequest, Task<TResponse>> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async Task<TResponse> Get(TOuterKey outerKey, TInnerRequest inner)
            {
                var task = cachedFunction.GetMany(outerKey, inner, CancellationToken.None);

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

        private Func<TOuterKey, IReadOnlyCollection<TInnerKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            async Task<IEnumerable<KeyValuePair<TInnerKey, TValue>>> Get(
                TOuterKey outerKey,
                IReadOnlyCollection<TInnerKey> innerKeys,
                CancellationToken cancellationToken)
            {
                if (!(innerKeys is TInnerRequest typedRequest))
                    typedRequest = requestConverter(innerKeys);

                var task = _originalFunction(outerKey, typedRequest);

                if (task.IsCompleted)
                    await task.ConfigureAwait(false);

                return task.Result;
            }
        }
    }
}