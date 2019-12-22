using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Configuration.EnumerableKeys;
using CacheMeIfYouCan.Configuration.SingleKey;

namespace CacheMeIfYouCan
{
    public static class CachedFunctionFactory
    {
        public static CachedFunctionConfigurationManagerAsync<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, Task<TValue>> originalFunction)
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, TValue> originalFunction)
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx<TKey, TValue>(originalFunction);
        }

        public static CachedFunctionConfigurationManagerAsync<TKey, TValue, TRequest, TResponse>
            ConfigureFor<TKey, TValue, TRequest, TResponse>(Func<TRequest, Task<TResponse>> originalFunction)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync<TKey, TValue, TRequest, TResponse>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue, TRequest, TResponse>
            ConfigureFor<TKey, TValue, TRequest, TResponse>(Func<TRequest, CancellationToken, Task<TResponse>> originalFunction)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx<TKey, TValue, TRequest, TResponse>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync<TKey, TValue, TRequest, TResponse>
            ConfigureFor<TKey, TValue, TRequest, TResponse>(Func<TRequest, TResponse> originalFunction)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync<TKey, TValue, TRequest, TResponse>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx<TKey, TValue, TRequest, TResponse>
            ConfigureFor<TKey, TValue, TRequest, TResponse>(Func<TRequest, CancellationToken, TResponse> originalFunction)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction == null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx<TKey, TValue, TRequest, TResponse>(originalFunction);
        }
    }
}