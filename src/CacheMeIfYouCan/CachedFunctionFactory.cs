using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.EnumerableKeys;
using CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys;
using CacheMeIfYouCan.Configuration.SingleKey;

namespace CacheMeIfYouCan
{
    public static class CachedFunctionFactory
    {
        public static CachedFunctionConfigurationManagerAsync<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx<TKey, TValue>(originalFunction);
        }

        public static CachedFunctionConfigurationManagerAsync<TRequest, TResponse, TKey, TValue>
            ConfigureFor<TRequest, TResponse, TKey, TValue>(Func<TRequest, Task<TResponse>> originalFunction)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync<TRequest, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx<TRequest, TResponse, TKey, TValue>
            ConfigureFor<TRequest, TResponse, TKey, TValue>(Func<TRequest, CancellationToken, Task<TResponse>> originalFunction)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx<TRequest, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync<TRequest, TResponse, TKey, TValue>
            ConfigureFor<TRequest, TResponse, TKey, TValue>(Func<TRequest, TResponse> originalFunction)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync<TRequest, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx<TRequest, TResponse, TKey, TValue>
            ConfigureFor<TRequest, TResponse, TKey, TValue>(Func<TRequest, CancellationToken, TResponse> originalFunction)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx<TRequest, TResponse, TKey, TValue>(originalFunction);
        }

        public static CachedFunctionConfigurationManagerAsync<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>
            ConfigureFor<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>(Func<TOuterKey, TInnerRequest, Task<TResponse>> originalFunction)
            where TInnerRequest : IEnumerable<TInnerKey>
            where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>
            ConfigureFor<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>(Func<TOuterKey, TInnerRequest, CancellationToken, Task<TResponse>> originalFunction)
            where TInnerRequest : IEnumerable<TInnerKey>
            where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>
            ConfigureFor<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>(Func<TOuterKey, TInnerRequest, TResponse> originalFunction)
            where TInnerRequest : IEnumerable<TInnerKey>
            where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>
            ConfigureFor<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>(Func<TOuterKey, TInnerRequest, CancellationToken, TResponse> originalFunction)
            where TInnerRequest : IEnumerable<TInnerKey>
            where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx<TOuterKey, TInnerRequest, TResponse, TInnerKey, TValue>(originalFunction);
        }
    }
}