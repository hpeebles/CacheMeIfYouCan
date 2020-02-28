using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public sealed class CachedFunctionConfigurationManagerAsyncCanx<TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerBase<TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx<TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue>>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        private readonly Func<TOuterKey, TInnerKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx(
            Func<TOuterKey, TInnerKeys, CancellationToken, Task<TResponse>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TOuterKey, TInnerKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async Task<TResponse> Get(TOuterKey outerKey, TInnerKeys innerKeys, CancellationToken cancellationToken)
            {
                var task = cachedFunction.GetMany(outerKey, innerKeys, cancellationToken);

                var results = task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);

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
                if (!(innerKeys is TInnerKeys typedRequest))
                    typedRequest = requestConverter(innerKeys);

                var task = _originalFunction(outerKey, typedRequest, cancellationToken);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
        }
    }
}