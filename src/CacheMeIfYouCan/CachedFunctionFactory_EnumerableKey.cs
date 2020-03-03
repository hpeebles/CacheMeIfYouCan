using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.EnumerableKeys;

namespace CacheMeIfYouCan
{
    public static partial class CachedFunctionFactory
    {
        #region Async
        public static CachedFunctionConfigurationManagerAsync<TKeys, TResponse, TKey, TValue>
            ConfigureFor<TKeys, TResponse, TKey, TValue>(Func<TKeys, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync<TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_2Params<TParam, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam, TKeys, TResponse, TKey, TValue>(Func<TParam, TKeys, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_2Params<TParam, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TKeys, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TKeys, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TKeys, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        #endregion
        
        #region AsyncCanx
        public static CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue>
            ConfigureFor<TKeys, TResponse, TKey, TValue>(Func<TKeys, CancellationToken, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam, TKeys, TResponse, TKey, TValue>(Func<TParam, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, Task<TResponse>> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        #endregion
        
        #region Sync
        public static CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue>
            ConfigureFor<TKeys, TResponse, TKey, TValue>(Func<TKeys, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam, TKeys, TResponse, TKey, TValue>(Func<TParam, TKeys, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TKeys, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TKeys, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        #endregion
        
        #region SyncCanx
        public static CachedFunctionConfigurationManagerSyncCanx<TKeys, TResponse, TKey, TValue>
            ConfigureFor<TKeys, TResponse, TKey, TValue>(Func<TKeys, CancellationToken, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx<TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam, TKeys, TResponse, TKey, TValue>(Func<TParam, TKeys, CancellationToken, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TKeys, CancellationToken, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>
            ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, TResponse> originalFunction)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(originalFunction);
        }
        #endregion
    }
}