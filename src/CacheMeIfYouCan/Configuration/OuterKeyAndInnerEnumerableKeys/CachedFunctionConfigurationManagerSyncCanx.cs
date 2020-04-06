using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerSyncCanxBase<TParams, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, TConfig>
        : CachedFunctionConfigurationManagerBase<TParams, TOuterKey, TInnerKeys, TResponse, TInnerKey, TValue, TConfig>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
        where TConfig : CachedFunctionConfigurationManagerSyncCanxBase<TParams, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, TConfig>
    {
        private readonly Func<TParams, TInnerKeys, CancellationToken, TResponse> _originalFunction;
        private readonly Func<TParams, TOuterKey> _keySelector;

        internal CachedFunctionConfigurationManagerSyncCanxBase(
            Func<TParams, TInnerKeys, CancellationToken, TResponse> originalFunction,
            Func<TParams, TOuterKey> keySelector)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;
        }

        private protected Func<TParams, TInnerKeys, CancellationToken, TResponse> BuildInternal()
        {
            var cachedFunction = BuildCachedFunction(ConvertFunction(), _keySelector);

            var responseConverter = GetResponseConverter();
            
            return Get;
            
            TResponse Get(TParams parameters, TInnerKeys innerKeys, CancellationToken cancellationToken)
            {
                var valueTask = cachedFunction.GetMany(parameters, innerKeys, cancellationToken);

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

        private Func<TParams, IReadOnlyCollection<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> ConvertFunction()
        {
            var requestConverter = GetRequestConverter();

            return Get;
            
            ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>> Get(
                TParams parameters,
                IReadOnlyCollection<TInnerKey> innerKeys,
                CancellationToken cancellationToken)
            {
                if (!(innerKeys is TInnerKeys typedRequest))
                    typedRequest = requestConverter(innerKeys);

                return new ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>(_originalFunction(parameters, typedRequest, cancellationToken));
            }
        }
    }

    public sealed class CachedFunctionConfigurationManagerSyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerSyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx(
            Func<TParam, TInnerKeys, CancellationToken, TResponse> originalFunc,
            Func<TParam, TOuterKey> keySelector)
            : base(originalFunc, keySelector)
        { }

        public CachedFunctionConfigurationManagerSyncCanx<TParam, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal(timeToLiveFactory);
        }

        public Func<TParam, TInnerKeys, CancellationToken, TResponse> Build() => BuildInternal();
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_3Params(
            Func<TParam1, TParam2, TInnerKeys, CancellationToken, TResponse> originalFunc,
            Func<TParam1, TParam2, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, keys));
        }

        public Func<TParam1, TParam2, TInnerKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, innerKeys, cancellationToken) => cachedFunction((p1, p2), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_4Params(
            Func<TParam1, TParam2, TParam3, TInnerKeys, CancellationToken, TResponse> originalFunc,
            Func<TParam1, TParam2, TParam3, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, keys));
        }

        public Func<TParam1, TParam2, TParam3, TInnerKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, innerKeys, cancellationToken) => cachedFunction((p1, p2, p3), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_5Params(
            Func<TParam1, TParam2, TParam3, TParam4, TInnerKeys, CancellationToken, TResponse> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TInnerKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, innerKeys, cancellationToken) => cachedFunction((p1, p2, p3, p4), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_6Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, CancellationToken, TResponse> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TInnerKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, innerKeys, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_7Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, CancellationToken, TResponse> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TInnerKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, innerKeys, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5, p6), innerKeys, cancellationToken);
        }
    }
    
    public sealed class CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>
        : CachedFunctionConfigurationManagerSyncCanxBase<(TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7), TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue, CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue>>,
            ICachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse>
        where TInnerKeys : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        internal CachedFunctionConfigurationManagerSyncCanx_8Params(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, CancellationToken, TResponse> originalFunc,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TOuterKey> keySelector)
            : base(
                (t, innerKeys, cancellationToken) => originalFunc(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, innerKeys, cancellationToken),
                t => keySelector(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7))
        { }

        public CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, TResponse, TOuterKey, TInnerKey, TValue> WithTimeToLiveFactory(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactoryInternal((t, keys) => timeToLiveFactory(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, keys));
        }

        public Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TInnerKeys, CancellationToken, TResponse> Build()
        {
            var cachedFunction = BuildInternal();

            return (p1, p2, p3, p4, p5, p6, p7, innerKeys, cancellationToken) => cachedFunction((p1, p2, p3, p4, p5, p6, p7), innerKeys, cancellationToken);
        }
    }
}