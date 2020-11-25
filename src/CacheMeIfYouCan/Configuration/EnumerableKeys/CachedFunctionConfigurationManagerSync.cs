using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys;

namespace CacheMeIfYouCan.Configuration.EnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerSyncBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        where TConfig : CachedFunctionConfigurationManagerBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, TKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSyncBase(Func<TParams, TKeys, TResponse> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        private protected Func<TParams, TKeys, TResponse> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            TResponse Get(TParams parameters, TKeys request)
            {
                var valueTask = cachedFunction.GetMany(parameters, request, CancellationToken.None);

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

        private Func<TParams, ReadOnlyMemory<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>> Get(
                TParams parameters,
                ReadOnlyMemory<TKey> keys,
                CancellationToken cancellationToken)
            {
                var typedRequest = requestConverter(keys);

                return new ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>(_originalFunction(parameters, typedRequest));
            }
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<Unit, TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_1Param<TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSync(Func<TKeys, TResponse> originalFunction)
            : base((_, keys) => originalFunction(keys))
        { }

        public CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((_, keys) => timeToLiveFactory(keys));
        }

        public CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((_, k) => predicate(k));
        }
        
        public CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((_, k, v) => predicate(k, v));
        }
        
        public CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((_, keys) => maxBatchSizeFactory(keys), batchBehaviour);
        }

        public Func<TKeys, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return keys => cachedFunction(null, keys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<TParam, TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam, TKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_2Params(Func<TParam, TKeys, TResponse> originalFunction)
            : base(originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerSync<TParam, TKeys, TResponse, TParam, TKey, TValue> UseFirstParamAsOuterCacheKey()
        {
            return new CachedFunctionConfigurationManagerSync<TParam, TKeys, TResponse, TParam, TKey, TValue>(_originalFunction, p => p);
        }
        
        public CachedFunctionConfigurationManagerSync<TParam, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSync<TParam, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }
        
        public CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }

        public CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal(maxBatchSizeFactory, batchBehaviour);
        }
        
        public Func<TParam, TKeys, TResponse> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_3Params(Func<TParam1, TParam2, TKeys, TResponse> originalFunction)
            : base((t, keys) => originalFunction(t.Item1, t.Item2, keys))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, keys));
        }
        
        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2));
        }

        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, k));
        }
        
        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2));
        }
        
        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, k, v));
        }

        public CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, keys), batchBehaviour);
        }
        
        public Func<TParam1, TParam2, TKeys, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, keys) => cachedFunction((p1, p2), keys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_4Params(Func<TParam1, TParam2, TParam3, TKeys, TResponse> originalFunction)
            : base((t, keys) => originalFunction(t.Item1, t.Item2, t.Item3, keys))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, keys));
        }
        
        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3));
        }

        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, k));
        }
        
        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3));
        }
        
        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, k, v));
        }

        public CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TKeys, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, keys) => cachedFunction((p1, p2, p3), keys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_5Params(Func<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> originalFunction)
            : base((t, keys) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, keys))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys));
        }
        
        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4));
        }

        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, k));
        }
        
        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4));
        }
        
        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, k, v));
        }

        public CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, TParam4, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (param1, p2, p3, p4, keys) => cachedFunction((param1, p2, p3, p4), keys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_6Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> originalFunction)
            : base((t, keys) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys));
        }
        
        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5));
        }

        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, k));
        }
        
        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5));
        }
        
        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, k, v));
        }

        public CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (param1, p2, p3, p4, p5, keys) => cachedFunction((param1, p2, p3, p4, p5), keys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_7Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> originalFunction)
            : base((t, keys) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys));
        }
        
        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6));
        }

        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, k));
        }
        
        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6));
        }
        
        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, k, v));
        }

        public CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (param1, p2, p3, p4, p5, p6, keys) => cachedFunction((param1, p2, p3, p4, p5, p6), keys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerSyncBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> _originalFunction;

        internal CachedFunctionConfigurationManagerSync_8Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> originalFunction)
            : base((t, keys) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys));
        }
        
        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7));
        }

        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, k));
        }
        
        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7));
        }
        
        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, k, v));
        }

        public CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (param1, p2, p3, p4, p5, p6, p7, keys) => cachedFunction((param1, p2, p3, p4, p5, p6, p7), keys);
        }
    }
}
