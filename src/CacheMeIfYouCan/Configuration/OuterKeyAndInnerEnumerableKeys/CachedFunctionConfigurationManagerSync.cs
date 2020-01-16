using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public sealed class CachedFunctionConfigurationManagerSync<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerBase<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue, CachedFunctionConfigurationManagerSync<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>>
        where TInnerRequest : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        private readonly Func<TOuterKey, TInnerRequest, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync(Func<TOuterKey, TInnerRequest, TResponse> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TOuterKey, TInnerRequest, TResponse> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            TResponse Get(TOuterKey outerKey, TInnerRequest inner)
            {
                var task = cachedFunction.GetMany(outerKey, inner, CancellationToken.None);

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

                return Task.FromResult((IEnumerable<KeyValuePair<TInnerKey, TValue>>)_originalFunction(outerKey, typedRequest));
            }
        }
    }
}