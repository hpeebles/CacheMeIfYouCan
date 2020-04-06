using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys;

namespace CacheMeIfYouCan.Configuration.EnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerSyncCanxBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        where TConfig : CachedFunctionConfigurationManagerBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, TKeys, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanxBase(Func<TParams, TKeys, CancellationToken, TResponse> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        private protected Func<TParams, TKeys, CancellationToken, TResponse> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            TResponse Get(TParams parameters, TKeys request, CancellationToken cancellationToken)
            {
                var valueTask = cachedFunction.GetMany(parameters, request, cancellationToken);

                var results = valueTask.IsCompleted
                    ? valueTask.Result
                    : valueTask.AsTask().GetAwaiter().GetResult();

                return results switch
                {
                    null => default,
                    TResponse typedResponse => typedResponse,
                    _ => responseConverter(results)
                };
            }
        }

        private Func<TParams, IReadOnlyCollection<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>> Get(
                TParams parameters,
                IReadOnlyCollection<TKey> keys,
                CancellationToken cancellationToken)
            {
                if (!(keys is TKeys typedRequest))
                    typedRequest = requestConverter(keys);

                return new ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>(_originalFunction(parameters, typedRequest, cancellationToken));
            }
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx<TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<Unit, TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSyncCanx<TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_1Param<TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx(Func<TKeys, CancellationToken, TResponse> originalFunction)
            : base((_, keys, cancellationToken) => originalFunction(keys, cancellationToken))
        { }

        public CachedFunctionConfigurationManagerSyncCanx<TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((_, keys) => timeToLiveFactory(keys));
        }

        public Func<TKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (keys, cancellationToken) => cachedFunction(null, keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<TParam, TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam, TKeys, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_2Params(Func<TParam, TKeys, CancellationToken, TResponse> originalFunction)
            : base(originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSyncCanx<TParam, TKeys, TResponse, TParam, TKey, TValue> UseFirstParamAsOuterCacheKey()
        {
            return new CachedFunctionConfigurationManagerSyncCanx<TParam, TKeys, TResponse, TParam, TKey, TValue>(_originalFunction, p => p);
        }
        
        public CachedFunctionConfigurationManagerSyncCanx<TParam, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx<TParam, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam, IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }
        
        public Func<TParam, TKeys, CancellationToken, TResponse> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TKeys, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_3Params(Func<TParam1, TParam2, TKeys, CancellationToken, TResponse> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, keys));
        }
        
        public Func<TParam1, TParam2, TKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, keys, cancellationToken) => cachedFunction((p1, p2), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_4Params(Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, TResponse> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, keys));
        }

        public Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, keys, cancellationToken) => cachedFunction((p1, p2, p3), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_5Params(Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, TResponse> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_6Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, TResponse> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_7Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, TResponse> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (param1, p2, p3, p4, p5, p6, keys, cancellationToken) =>
                cachedFunction((param1, p2, p3, p4, p5, p6), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncCanx_8Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, TResponse> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (param1, p2, p3, p4, p5, p6, p7, keys, cancellationToken) =>
                cachedFunction((param1, p2, p3, p4, p5, p6, p7), keys, cancellationToken);
        }
    }
}