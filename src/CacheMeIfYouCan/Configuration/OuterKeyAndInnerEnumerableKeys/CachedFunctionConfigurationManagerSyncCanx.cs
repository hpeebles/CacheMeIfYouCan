using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public sealed class CachedFunctionConfigurationManagerSyncCanx<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerBase<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue, CachedFunctionConfigurationManagerSyncCanx<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>>
        where TInnerRequest : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        private readonly Func<TOuterKey, TInnerRequest, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx(
            Func<TOuterKey, TInnerRequest, CancellationToken, TResponse> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TOuterKey, TInnerRequest, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            TResponse Get(TOuterKey outerKey, TInnerRequest innerKeys, CancellationToken cancellationToken)
            {
                var task = cachedFunction.GetMany(outerKey, innerKeys, cancellationToken);

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

        private Func<TOuterKey, IReadOnlyCollection<TInnerKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            Task<IEnumerable<KeyValuePair<TInnerKey, TValue>>> Get(
                TOuterKey outerKey,
                IReadOnlyCollection<TInnerKey> innerKeys,
                CancellationToken cancellationToken)
            {
                if (!(innerKeys is TInnerRequest typedRequest))
                    typedRequest = requestConverter(innerKeys);

                return Task.FromResult((IEnumerable<KeyValuePair<TInnerKey, TValue>>)_originalFunction(outerKey, typedRequest, cancellationToken));
            }
        }
    }
}