﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys;

namespace CacheMeIfYouCan.Configuration.EnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerAsyncCanxBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        where TConfig : CachedFunctionConfigurationManagerBase<TParams, TKeys, TResponse, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, TKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanxBase(Func<TParams, TKeys, CancellationToken, Task<TResponse>> originalFunction)
        {
            _originalFunction = originalFunction;
        }

        private protected Func<TParams, TKeys, CancellationToken, Task<TResponse>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction());

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async Task<TResponse> Get(TParams parameters, TKeys request, CancellationToken cancellationToken)
            {
                var valueTask = cachedFunction.GetMany(parameters, request, cancellationToken);

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

        private Func<TParams, ReadOnlyMemory<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            async ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>> Get(
                TParams parameters,
                ReadOnlyMemory<TKey> keys,
                CancellationToken cancellationToken)
            {
                var typedRequest = requestConverter(keys);

                var task = _originalFunction(parameters, typedRequest, cancellationToken);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<Unit, TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_1Param<TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx(Func<TKeys, CancellationToken, Task<TResponse>> originalFunction)
            : base((_, keys, cancellationToken) => originalFunction(keys, cancellationToken))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((_, keys) => timeToLiveFactory(keys));
        }

        public CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((_, k) => predicate(k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((_, k, v) => predicate(k, v));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((_, keys) => maxBatchSizeFactory(keys), batchBehaviour);
        }

        public Func<TKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (keys, cancellationToken) => cachedFunction(null, keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<TParam, TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam, TKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_2Params(Func<TParam, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            : base(originalFunction)
        {
            _originalFunction = originalFunction;
        }

        public CachedFunctionConfigurationManagerAsyncCanx<TParam, TKeys, TResponse, TParam, TKey, TValue> UseFirstParamAsOuterCacheKey()
        {
            return new CachedFunctionConfigurationManagerAsyncCanx<TParam, TKeys, TResponse, TParam, TKey, TValue>(_originalFunction, p => p);
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx<TParam, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx<TParam, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }

        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal(maxBatchSizeFactory, batchBehaviour);
        }
        
        public Func<TParam, TKeys, CancellationToken, Task<TResponse>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_3Params(Func<TParam1, TParam2, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, k, v));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, keys), batchBehaviour);
        }
        
        public Func<TParam1, TParam2, TKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, keys, cancellationToken) => cachedFunction((p1, p2), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_4Params(Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, k, v));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, keys, cancellationToken) => cachedFunction((p1, p2, p3), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_5Params(Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, k, v));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, TParam4, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_6Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, k, v));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_7Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, k, v));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, keys, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5, p6), keys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKeys, TResponse, TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>
        where TKeys : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, Task<TResponse>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_8Params(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            : base((t, keys, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys, cancellationToken))
        {
            _originalFunction = originalFunction;
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TOuterKey, TKey, TValue> WithOuterCacheKey<TOuterKey>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TOuterKey> keySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TOuterKey, TKey, TValue>(_originalFunction, keySelector);
        }

        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, bool> predicate)
        {
            return DontGetFromCacheWhenInternal((p, k) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, k));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(p => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((p, k, v) => predicate(p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6, p.Item7, k, v));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithBatchedFetches(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ReadOnlyMemory<TKey>, int> maxBatchSizeFactory, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            return WithBatchedFetchesInternal((t, keys) => maxBatchSizeFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys), batchBehaviour);
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, Task<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (param1, p2, p3, p4, p5, p6, p7, keys, cancellationToken) =>
                cachedFunction((param1, p2, p3, p4, p5, p6, p7), keys, cancellationToken);
        }
    }
}
