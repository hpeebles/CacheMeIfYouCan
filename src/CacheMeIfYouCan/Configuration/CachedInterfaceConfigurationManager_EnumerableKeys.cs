using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.EnumerableKeys;
using CacheMeIfYouCan.Configuration.SingleKey;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public sealed partial class CachedInterfaceConfigurationManager<T>
    {
        #region Async
        public CachedInterfaceConfigurationManager<T> Configure<TRequest, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TRequest, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync<TRequest, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsync_1Param<TRequest, TResponse>> configurationFunc)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam, TKeys, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_2Params<TParam, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsync_2Params<TParam, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TKeys, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TKeys, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TKeys, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion

        #region AsyncCanx
        public CachedInterfaceConfigurationManager<T> Configure<TRequest, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TRequest, CancellationToken, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx<TRequest, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_1Param<TRequest, TResponse>> configurationFunc)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam, TKeys, CancellationToken, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_2Params<TParam, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TKeys, CancellationToken, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, Task<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion

        #region Sync
        public CachedInterfaceConfigurationManager<T> Configure<TRequest, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TRequest, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSync<TRequest, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSync_1Param<TRequest, TResponse>> configurationFunc)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam, TKeys, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSync_2Params<TParam, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TKeys, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TKeys, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion

        #region SyncCanx
        public CachedInterfaceConfigurationManager<T> Configure<TRequest, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TRequest, CancellationToken, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx<TRequest, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_1Param<TRequest, TResponse>> configurationFunc)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam, TKeys, CancellationToken, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_2Params<TParam, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TKeys, CancellationToken, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, TResponse>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion
        
        #region ValueTask
        public CachedInterfaceConfigurationManager<T> Configure<TRequest, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TRequest, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask<TRequest, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_1Param<TRequest, TResponse>> configurationFunc)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam, TKeys, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_2Params<TParam, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_2Params<TParam, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TKeys, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TKeys, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TKeys, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion

        #region ValueTaskCanx
        public CachedInterfaceConfigurationManager<T> Configure<TRequest, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TRequest, CancellationToken, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx<TRequest, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TRequest, TResponse>> configurationFunc)
            where TRequest : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam, TKeys, CancellationToken, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TKeys, CancellationToken, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TKeys, CancellationToken, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TKeys, CancellationToken, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, CancellationToken, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, CancellationToken, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, CancellationToken, ValueTask<TResponse>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse, TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TKeys, TResponse>> configurationFunc)
            where TKeys : IEnumerable<TKey>
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion
    }
}