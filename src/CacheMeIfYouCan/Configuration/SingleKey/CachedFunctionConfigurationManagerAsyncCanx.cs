﻿﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public abstract class CachedFunctionConfigurationManagerAsyncCanxBase<TParams, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, CancellationToken, Task<TValue>> _originalFunction;
        private readonly Func<TParams, TKey> _cacheKeySelector;

        internal CachedFunctionConfigurationManagerAsyncCanxBase(
            Func<TParams, CancellationToken, Task<TValue>> originalFunction,
            Func<TParams, TKey> cacheKeySelector)
        {
            _originalFunction = originalFunction;
            _cacheKeySelector = cacheKeySelector;
        }

        private protected Func<TParams, CancellationToken, Task<TValue>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction(), _cacheKeySelector);

            return Get;
            
            Task<TValue> Get(TParams parameters, CancellationToken cancellationToken)
            {
                return cachedFunction.Get(parameters, cancellationToken).AsTask();
            }
        }
        
        private Func<TParams, CancellationToken, ValueTask<TValue>> ConvertFunction()
        {
            return (keys, cancellationToken) => new ValueTask<TValue>(_originalFunction(keys, cancellationToken));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> :
        CachedFunctionConfigurationManagerAsyncCanxBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue>>,
        ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<TParam, TValue>
    {
        private readonly Func<TParam, CancellationToken, Task<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerAsyncCanx_1Param(
            Func<TParam, CancellationToken, Task<TValue>> originalFunction)
            : base(originalFunction, p => p)
        {
            _originalFunction = originalFunction;
        }

        public ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>(_originalFunction, cacheKeySelector);
        }

        public ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> WithTimeToLiveFactory(
            Func<TParam, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }

        public ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> DontStoreInCacheWhen(
            Func<TParam, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }

        public Func<TParam, CancellationToken, Task<TValue>> Build() => BuildInternal();
        
        internal Func<TParam, CancellationToken, Task<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>>,
            ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_1Param(
            Func<TParam, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam, TKey> cacheKeySelector)
            : base(originalFunction, cacheKeySelector)
        { }

        public ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }

        public ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public Func<TParam, CancellationToken, Task<TValue>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_2Params(
            Func<TParam1, TParam2, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2));
        }

        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, value));
        }


        public Func<TParam1, TParam2, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, cancellationToken) => cachedFunction((p1, p2), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_3Params(
            Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, value));
        }
        
        public Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, cancellationToken) => cachedFunction((p1, p2, p3), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_4Params(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, value));
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, cancellationToken) => cachedFunction((p1, p2, p3, p4), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, value));
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5, p6), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5, p6, p7), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>
        : CachedFunctionConfigurationManagerAsyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue, CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>>,
            ICachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>
    {
        internal CachedFunctionConfigurationManagerAsyncCanx_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8))
        { }

        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8));
        }
        
        public CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) =>
                cachedFunction((p1, p2, p3, p4, p5, p6, p7, p8), cancellationToken);
        }
    }
}