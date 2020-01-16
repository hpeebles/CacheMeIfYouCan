using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public sealed class CachedFunctionConfigurationManagerAsyncCanx<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerBase<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>>
        where TInnerRequest : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        private readonly Func<TOuterKey, TInnerRequest, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx(
            Func<TOuterKey, TInnerRequest, CancellationToken, Task<TResponse>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TOuterKey, TInnerRequest, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async Task<TResponse> Get(TOuterKey outerKey, TInnerRequest innerKeys, CancellationToken cancellationToken)
            {
                var task = cachedFunction.GetMany(outerKey, innerKeys, cancellationToken);

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

                var task = _originalFunction(outerKey, typedRequest, cancellationToken);

                if (task.IsCompleted)
                    await task.ConfigureAwait(false);

                return task.Result;
            }
        }
    }
}