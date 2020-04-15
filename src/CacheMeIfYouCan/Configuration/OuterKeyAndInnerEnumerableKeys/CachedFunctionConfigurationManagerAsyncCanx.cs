using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerAsyncCanxBase<TParams, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue, TConfig>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
        where TConfig : CachedFunctionConfigurationManagerAsyncCanxBase<TParams, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, TConfig>
    {
        private readonly Func<TParams, TInnerKeys, CancellationToken, Task<TResponse>> _originalFunction;
        private readonly Func<TParams, TOuterKey> _keySelector;

        internal CachedFunctionConfigurationManagerAsyncCanxBase(
            Func<TParams, TInnerKeys, CancellationToken, Task<TResponse>> originalFunction,
            Func<TParams, TOuterKey> keySelector)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;
        }

        private protected Func<TParams, TInnerKeys, CancellationToken, Task<TResponse>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction(), _keySelector);

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async Task<TResponse> Get(TParams parameters, TInnerKeys innerKeys, CancellationToken cancellationToken)
            {
                var valueTask = cachedFunction.GetMany(parameters, innerKeys, cancellationToken);

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

                var task = _originalFunction(parameters, typedRequest, cancellationToken);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
        }
    }

    public sealed class CachedFunctionConfigurationManagerAsyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx(
            Func<TParam, TInnerKeys, CancellationToken, Task<TResponse>> originalFunc,
            Func<TParam, TOuterKey> keySelector)
            : base(originalFunc, keySelector)
        { }

        public CachedFunctionConfigurationManagerAsyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }

        public CachedFunctionConfigurationManagerAsyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }

        public Func<TParam, TInnerKeys, CancellationToken, Task<TResponse>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_3Params(
            Func<TParam1, TParam2, TInnerKeys, CancellationToken, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, k, v));
        }

        public Func<TParam1, TParam2, TInnerKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, innerKeys, cancellationToken) =>
                cachedFunction((p1, p2), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_4Params(
            Func<TParam1, TParam2, TParam3, TInnerKeys, CancellationToken, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TInnerKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, innerKeys, cancellationToken) =>
                cachedFunction((p1, p2, p3), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TInnerKeys, CancellationToken, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TInnerKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, innerKeys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, CancellationToken, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, innerKeys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, CancellationToken, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, innerKeys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5, p6), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, CancellationToken, Task<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, k, v));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, innerKeys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5, p6, p7), innerKeys, cancellationToken);
        }
    }
}