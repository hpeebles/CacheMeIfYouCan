﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public abstract class CachedFunctionConfigurationManagerValueTaskCanxBase<TParams, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, CancellationToken, ValueTask<TValue>> _originalFunction;
        private readonly Func<TParams, TKey> _cacheKeySelector;

        internal CachedFunctionConfigurationManagerValueTaskCanxBase(
            Func<TParams, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParams, TKey> cacheKeySelector)
        {
            _originalFunction = originalFunction;
            _cacheKeySelector = cacheKeySelector;
        }

        private protected Func<TParams, CancellationToken, ValueTask<TValue>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(_originalFunction, _cacheKeySelector);

            return cachedFunction.Get;
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> :
        CachedFunctionConfigurationManagerValueTaskCanxBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue>>,
        ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param_KeySelector<TParam, TValue>
    {
        private readonly Func<TParam, CancellationToken, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTaskCanx_1Param(
            Func<TParam, CancellationToken, ValueTask<TValue>> originalFunction)
            : base(originalFunction, p => p)
        {
            _originalFunction = originalFunction;
        }

        public ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>(_originalFunction, cacheKeySelector);
        }

        public ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> WithTimeToLiveFactory(
            Func<TParam, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }

        public ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> DontStoreInCacheWhen(
            Func<TParam, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public Func<TParam, CancellationToken, ValueTask<TValue>> Build() => BuildInternal();
        
        internal Func<TParam, CancellationToken, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>>,
            ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_1Param(
            Func<TParam, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam, TKey> cacheKeySelector)
            : base(originalFunction, cacheKeySelector)
        { }

        public ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }

        public ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public Func<TParam, CancellationToken, ValueTask<TValue>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_2Params(
            Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2));
        }

        public CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, value));
        }

        public Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, cancellationToken) => cachedFunction((p1, p2), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_3Params(
            Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, value));
        }
        
        public Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, cancellationToken) => cachedFunction((p1, p2, p3), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_4Params(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, value));
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, cancellationToken) => cachedFunction((p1, p2, p3, p4), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, value));
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5, p6), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5, p6, p7), cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue, CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTaskCanx_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
            : base(
                (t, cancellationToken) => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, cancellationToken),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8))
        { }

        public CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8));
        }
        
        public CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5, p6, p7, p8), cancellationToken);
        }
    }
}