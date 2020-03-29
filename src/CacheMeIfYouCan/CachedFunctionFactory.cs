using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.SingleKey;

namespace CacheMeIfYouCan
{
    public static class CachedFunctionFactory
    {
        #region Async
        public static ISingleKeyCachedFunctionConfigurationManagerAsync_1Param_KeySelector<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_1Param<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_2Params_KeySelector<TParam1, TParam2, TValue> ConfigureFor<TParam1, TParam2, TValue>(
            Func<TParam1, TParam2, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_2Params_KeySelector<TParam1, TParam2, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_3Params_KeySelector<TParam1, TParam2, TParam3, TValue> ConfigureFor<TParam1, TParam2, TParam3, TValue>(
            Func<TParam1, TParam2, TParam3, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(originalFunction);
        }
        #endregion

        #region AsyncCanx
        public static ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_1Param<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_2Params_KeySelector<TParam1, TParam2, TValue> ConfigureFor<TParam1, TParam2, TValue>(
            Func<TParam1, TParam2, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_2Params_KeySelector<TParam1, TParam2, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue> ConfigureFor<TParam1, TParam2, TParam3, TValue>(
            Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerAsyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(originalFunction);
        }
        #endregion
        
        #region Sync
        public static ISingleKeyCachedFunctionConfigurationManagerSync_1Param_KeySelector<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_1Param<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_2Params_KeySelector<TParam1, TParam2, TValue> ConfigureFor<TParam1, TParam2, TValue>(
            Func<TParam1, TParam2, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_2Params_KeySelector<TParam1, TParam2, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_3Params_KeySelector<TParam1, TParam2, TParam3, TValue> ConfigureFor<TParam1, TParam2, TParam3, TValue>(
            Func<TParam1, TParam2, TParam3, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(originalFunction);
        }
        #endregion

        #region SyncCanx
        public static ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_1Param<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_2Params_KeySelector<TParam1, TParam2, TValue> ConfigureFor<TParam1, TParam2, TValue>(
            Func<TParam1, TParam2, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_2Params_KeySelector<TParam1, TParam2, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue> ConfigureFor<TParam1, TParam2, TParam3, TValue>(
            Func<TParam1, TParam2, TParam3, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerSyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(originalFunction);
        }
        #endregion

        #region ValueTask
        public static ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param_KeySelector<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTask_1Param<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_2Params_KeySelector<TParam1, TParam2, TValue> ConfigureFor<TParam1, TParam2, TValue>(
            Func<TParam1, TParam2, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTask_2Params_KeySelector<TParam1, TParam2, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_3Params_KeySelector<TParam1, TParam2, TParam3, TValue> ConfigureFor<TParam1, TParam2, TParam3, TValue>(
            Func<TParam1, TParam2, TParam3, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTask_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTask_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTask_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTask_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTask_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTask_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(originalFunction);
        }
        #endregion

        #region ValueTaskCanx
        public static ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param_KeySelector<TKey, TValue> ConfigureFor<TKey, TValue>(
            Func<TKey, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTaskCanx_1Param<TKey, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_2Params_KeySelector<TParam1, TParam2, TValue> ConfigureFor<TParam1, TParam2, TValue>(
            Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTaskCanx_2Params_KeySelector<TParam1, TParam2, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue> ConfigureFor<TParam1, TParam2, TParam3, TValue>(
            Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTaskCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTaskCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTaskCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTaskCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTaskCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(originalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue> ConfigureFor<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>> originalFunction)
        {
            if (originalFunction is null)
                throw new ArgumentNullException(nameof(originalFunction));
            
            return new CachedFunctionConfigurationManagerValueTaskCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(originalFunction);
        }
        #endregion
    }
}