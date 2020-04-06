using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerValueTaskBase<TParams, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue, TConfig>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
        where TConfig : CachedFunctionConfigurationManagerValueTaskBase<TParams, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, TConfig>
    {
        private readonly Func<TParams, TInnerKeys, ValueTask<TResponse>> _originalFunction;
        private readonly Func<TParams, TOuterKey> _keySelector;

        internal CachedFunctionConfigurationManagerValueTaskBase(
            Func<TParams, TInnerKeys, ValueTask<TResponse>> originalFunction,
            Func<TParams, TOuterKey> keySelector)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;
        }

        private protected Func<TParams, TInnerKeys, ValueTask<TResponse>> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction(), _keySelector);

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            async ValueTask<TResponse> Get(TParams parameters, TInnerKeys innerKeys)
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

    public sealed class CachedFunctionConfigurationManagerValueTask<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerValueTask<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_2Params<TParam, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTask(
            Func<TParam, TInnerKeys, ValueTask<TResponse>> originalFunc,
            Func<TParam, TOuterKey> keySelector)
            : base(originalFunc, keySelector)
        { }

        public CachedFunctionConfigurationManagerValueTask<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }

        public Func<TParam, TInnerKeys, ValueTask<TResponse>> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTask_3Params(
            Func<TParam1, TParam2, TInnerKeys, ValueTask<TResponse>> originalFunc,
            Func<TParam1, TParam2, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, innerKeys),
                t => keySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, keys));
        }

        public Func<TParam1, TParam2, TInnerKeys, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, innerKeys) => cachedFunction((p1, p2), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTask_4Params(
            Func<TParam1, TParam2, TParam3, TInnerKeys, ValueTask<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, keys));
        }

        public Func<TParam1, TParam2, TParam3, TInnerKeys, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, innerKeys) => cachedFunction((p1, p2, p3), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTask_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TInnerKeys, ValueTask<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }

        public CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TInnerKeys, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, innerKeys) =>
                cachedFunction((p1, p2, p3, p4), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTask_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, ValueTask<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }

        public CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, innerKeys) => 
                cachedFunction((p1, p2, p3, p4, p5), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTask_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, ValueTask<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }

        public CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, innerKeys) =>
                cachedFunction((p1, p2, p3, p4, p5, p6), innerKeys);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerValueTaskBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerValueTask_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, ValueTask<TResponse>> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TOuterKey> keySelector)
            : base(
                (t, innerKeys) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, innerKeys),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }

        public CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, ValueTask<TResponse>> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, innerKeys) =>
                cachedFunction((p1, p2, p3, p4, p5, p6, p7), innerKeys);
        }
    }
}