using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerAsyncBase<TParams, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue, TConfig>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
        where TConfig : CachedFunctionConfigurationManagerAsyncBase<TParams, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, TConfig>
    {
        private readonly Func<TParams, TInnerKeys, Task<TResponse>> _originalFunction;
        private readonly Func<TParams, TOuterKey> _keySelector;

        internal CachedFunctionConfigurationManagerAsyncBase(
            Func<TParams, TInnerKeys, Task<TResponse>> originalFunction,
            Func<TParams, TOuterKey> keySelector)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;
        }

        private protected Func<TParams, TInnerKeys, Task<TResponse>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction(), _keySelector);

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async Task<TResponse> Get(TParams parameters, TInnerKeys innerKeys)
            {
                var valueTask = cachedFunction.GetMany(parameters, innerKeys, CancellationToken.None);

                var results = valueTask.IsCompleted
                    ? valueTask.Result
                    : await valueTask.ConfigureAwait(false);

                return results switch
                {
                    null => default,
                    TResponse typedResponse => typedResponse,
                    _ => responseConverter(results)
                };
            }
        }

        private Func<TParams, IReadOnlyCollection<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            async ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>> Get(
                TParams parameters,
                IReadOnlyCollection<TInnerKey> innerKeys,
                CancellationToken cancellationToken)
            {
                if (!(innerKeys is TInnerKeys typedRequest))
                    typedRequest = requestConverter(innerKeys);

                var task = _originalFunction(parameters, typedRequest);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
        }
    }

    public sealed class CachedFunctionConfigurationManagerAsync<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncBase<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsync<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsync_2Params<TParam, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsync(
            Func<TParam, TInnerKeys, Task<TResponse>> originalFunc,
            Func<TParam, TOuterKey> keySelector)
            : base(originalFunc, keySelector)
        { }

        public CachedFunctionConfigurationManagerAsync<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }
        
        public CachedFunctionConfigurationManagerAsync<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }

        public CachedFunctionConfigurationManagerAsync<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerAsync<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerAsync<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }

        public Func<TParam, TInnerKeys, Task<TResponse>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncBase<(TParam1, TParam2), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsync_3Params(
            Func<TParam1, TParam2, TInnerKeys, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, innerKeys),
                t => keySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, keys));
        }
        
        public CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2));
        }

        public CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, k));
        }
        
        public CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2));
        }
        
        public CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, k, v));
        }

        public Func<TParam1, TParam2, TInnerKeys, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, innerKeys) => cachedFunction((p1, p2), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncBase<(TParam1, TParam2, TParam3), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsync_4Params(
            Func<TParam1, TParam2, TParam3, TInnerKeys, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, keys));
        }
        
        public CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3));
        }

        public CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, k));
        }
        
        public CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3));
        }
        
        public CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TInnerKeys, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, innerKeys) => cachedFunction((p1, p2, p3), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncBase<(TParam1, TParam2, TParam3, TParam4), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsync_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TInnerKeys, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }

        public CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys));
        }
        
        public CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4));
        }

        public CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, k));
        }
        
        public CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4));
        }
        
        public CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TInnerKeys, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, innerKeys) =>
                cachedFunction((p1, p2, p3, p4), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsync_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }

        public CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys));
        }
        
        public CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5));
        }

        public CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, k));
        }
        
        public CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5));
        }
        
        public CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, innerKeys) => 
                cachedFunction((p1, p2, p3, p4, p5), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsync_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }

        public CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys));
        }
        
        public CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6));
        }

        public CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, k));
        }
        
        public CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6));
        }
        
        public CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, innerKeys) =>
                cachedFunction((p1, p2, p3, p4, p5, p6), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsync_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }

        public CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys));
        }
        
        public CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7));
        }

        public CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, k));
        }
        
        public CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7));
        }
        
        public CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, innerKeys) =>
                cachedFunction((p1, p2, p3, p4, p5, p6, p7), innerKeys);
        }
    }
}