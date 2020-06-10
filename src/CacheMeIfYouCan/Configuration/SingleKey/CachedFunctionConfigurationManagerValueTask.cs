﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public abstract class CachedFunctionConfigurationManagerValueTaskBase<TParams, TKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        private readonly Func<TParams, ValueTask<TValue>> _originalFunction;
        private readonly Func<TParams, TKey> _cacheKeySelector;
        
        internal CachedFunctionConfigurationManagerValueTaskBase(
            Func<TParams, ValueTask<TValue>> originalFunction,
            Func<TParams, TKey> cacheKeySelector)
        {
            _originalFunction = originalFunction;
            _cacheKeySelector = cacheKeySelector;
        }

        private protected Func<TParams, ValueTask<TValue>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction(), _cacheKeySelector);

            return Get;
            
            ValueTask<TValue> Get(TParams parameters)
            {
                return cachedFunction.Get(parameters, CancellationToken.None);
            }
        }

        private Func<TParams, CancellationToken, ValueTask<TValue>> ConvertFunction()
        {
            return (keys, _) => _originalFunction(keys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue> :
        CachedFunctionConfigurationManagerValueTaskBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue>>,
        ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param_KeySelector<TParam, TValue>
    {
        private readonly Func<TParam, ValueTask<TValue>> _originalFunction;

        internal CachedFunctionConfigurationManagerValueTask_1Param(
            Func<TParam, ValueTask<TValue>> originalFunction)
            : base(originalFunction, p => p)
        {
            _originalFunction = originalFunction;
        }

        public ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(
            Func<TParam, TKey> cacheKeySelector)
        {
            return new CachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue>(_originalFunction, cacheKeySelector);
        }

        public ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue> WithTimeToLiveFactory(
            Func<TParam, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }

        public ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue> DontStoreInCacheWhen(
            Func<TParam, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public Func<TParam, ValueTask<TValue>> Build() => BuildInternal();
        
        internal Func<TParam, ValueTask<TValue>> OriginalFunction => _originalFunction;
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue>>,
            ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTask_1Param(
            Func<TParam, ValueTask<TValue>> originalFunction,
            Func<TParam, TKey> cacheKeySelector)
            : base(originalFunction, cacheKeySelector)
        { }

        public ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }

        public ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(predicate);
        }
        
        public ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal(predicate);
        }
        
        public Func<TParam, ValueTask<TValue>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_2Params<TParam1, TParam2, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2), TKey, TValue, CachedFunctionConfigurationManagerValueTask_2Params<TParam1, TParam2, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_2Params<TParam1, TParam2, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTask_2Params(
            Func<TParam1, TParam2, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2),
                t => cacheKeySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerValueTask_2Params<TParam1, TParam2, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2));
        }

        public CachedFunctionConfigurationManagerValueTask_2Params<TParam1, TParam2, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2));
        }
        
        public CachedFunctionConfigurationManagerValueTask_2Params<TParam1, TParam2, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, value));
        }

        public Func<TParam1, TParam2, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2) => cachedFunction((p1, p2));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TParam3, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3), TKey, TValue, CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TParam3, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TParam3, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTask_3Params(
            Func<TParam1, TParam2, TParam3, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TParam3, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3));
        }
        
        public CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TParam3, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3));
        }
        
        public CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TParam3, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, value));
        }
        
        public Func<TParam1, TParam2, TParam3, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3) => cachedFunction((p1, p2, p3));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4), TKey, TValue, CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TParam4, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTask_4Params(
            Func<TParam1, TParam2, TParam3, TParam4, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }
        
        public CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4));
        }
        
        public CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4));
        }
        
        public CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TParam4, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, value));
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4) => cachedFunction((p1, p2, p3, p4));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TKey, TValue, CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTask_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }
        
        public CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }
        
        public CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5));
        }
        
        public CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, value));
        }
        
        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5) => cachedFunction((p1, p2, p3, p4, p5));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TKey, TValue, CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTask_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }
        
        public CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6));
        }
        
        public CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6));
        }
        
        public CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6) => cachedFunction((p1, p2, p3, p4, p5, p6));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TKey, TValue, CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTask_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }
        
        public CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7));
        }
        
        public CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7));
        }
        
        public CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7) => cachedFunction((p1, p2, p3, p4, p5, p6, p7));
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8), TKey, TValue, CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>
    {
        internal CachedFunctionConfigurationManagerValueTask_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, ValueTask<TValue>> originalFunction,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey> cacheKeySelector)
            : base(
                t => originalFunction(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8),
                t => cacheKeySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8))
        { }

        public CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(t => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8));
        }
        
        public CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> DontGetFromCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool> predicate)
        {
            return DontGetFromCacheWhenInternal(t => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8));
        }
        
        public CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TKey, TValue> DontStoreInCacheWhen(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue, bool> predicate)
        {
            return DontStoreInCacheWhenInternal((t, value) => predicate(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, value));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, ValueTask<TValue>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, p8) => cachedFunction((p1, p2, p3, p4, p5, p6, p7, p8));
        }
    }
}