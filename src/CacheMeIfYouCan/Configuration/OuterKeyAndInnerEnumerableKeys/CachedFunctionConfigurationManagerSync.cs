using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public sealed class CachedFunctionConfigurationManagerSync<TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerBase<TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue, CachedFunctionConfigurationManagerSync<TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue>>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        private readonly Func<TOuterKey, TInnerKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync(Func<TOuterKey, TInnerKeys, TResponse> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public Func<TOuterKey, TInnerKeys, TResponse> Build()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            TResponse Get(TOuterKey outerKey, TInnerKeys innerKeys)
            {
                var task = cachedFunction.GetMany(outerKey, innerKeys, CancellationToken.None);

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
                if (!(innerKeys is TInnerKeys typedRequest))
                    typedRequest = requestConverter(innerKeys);

                return Task.FromResult((IEnumerable<KeyValuePair<TInnerKey, TValue>>)_originalFunction(outerKey, typedRequest));
            }
        }
    }
}