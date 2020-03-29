using System.Collections.Generic;
using CacheMeIfYouCan.Configuration.EnumerableKeys;
using CacheMeIfYouCan.Configuration.SingleKey;

namespace CacheMeIfYouCan
{
    public static class CachedFunctionConfigurationManagerExtensions
    {
        public static CachedFunctionConfigurationManagerAsync<TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TKeys, TResponse, TKey, TValue>(
            this ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsync<TKeys, TResponse, TKey, TValue>(((CachedFunctionConfigurationManagerAsync_1Param<TKeys, TResponse>)config).OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_2Params<TParam, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsync_2Params_KeySelector<TParam, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsync_2Params<TParam, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsync_3Params_KeySelector<TParam1, TParam2, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsync_4Params_KeySelector<TParam1, TParam2, TParam3, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TKeys, TResponse, TKey, TValue>(
            this ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsyncCanx<TKeys, TResponse, TKey, TValue>(((CachedFunctionConfigurationManagerAsyncCanx_1Param<TKeys, TResponse>)config).OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsyncCanx_2Params_KeySelector<TParam, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsyncCanx_3Params_KeySelector<TParam1, TParam2, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerAsyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TKeys, TResponse, TKey, TValue>(
            this ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSync<TKeys, TResponse, TKey, TValue>(((CachedFunctionConfigurationManagerSync_1Param<TKeys, TResponse>)config).OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSync_2Params_KeySelector<TParam, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSync_3Params_KeySelector<TParam1, TParam2, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSync_4Params_KeySelector<TParam1, TParam2, TParam3, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx<TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TKeys, TResponse, TKey, TValue>(
            this ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSyncCanx<TKeys, TResponse, TKey, TValue>(((CachedFunctionConfigurationManagerSyncCanx_1Param<TKeys, TResponse>)config).OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSyncCanx_2Params_KeySelector<TParam, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSyncCanx_3Params_KeySelector<TParam1, TParam2, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerSyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask<TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TKeys, TResponse, TKey, TValue>(
            this ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTask<TKeys, TResponse, TKey, TValue>(((CachedFunctionConfigurationManagerValueTask_1Param<TKeys, TResponse>)config).OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_2Params<TParam, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTask_2Params_KeySelector<TParam, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTask_2Params<TParam, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTask_3Params_KeySelector<TParam1, TParam2, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTask_4Params_KeySelector<TParam1, TParam2, TParam3, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTask_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTask_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTask_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTask_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx<TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TKeys, TResponse, TKey, TValue>(
            this ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx<TKeys, TResponse, TKey, TValue>(((CachedFunctionConfigurationManagerValueTaskCanx_1Param<TKeys, TResponse>)config).OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTaskCanx_2Params_KeySelector<TParam, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTaskCanx_3Params_KeySelector<TParam1, TParam2, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTaskCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTaskCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTaskCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTaskCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
        
        public static CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue> WithEnumerableKeys<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            this CachedFunctionConfigurationManagerValueTaskCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse> config)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            return new CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(config.OriginalFunction);
        }
    }
}