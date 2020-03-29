using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.SingleKey;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public sealed partial class CachedInterfaceConfigurationManager<T>
    {
        private readonly T _originalImpl;
        private readonly HashSet<MethodInfo> _allInterfaceMethods; 
        private readonly Dictionary<MethodInfo, object> _functionCacheConfigurationFunctions;

        internal CachedInterfaceConfigurationManager(T originalImpl)
        {
            _originalImpl = originalImpl;
            _allInterfaceMethods = new HashSet<MethodInfo>(InterfaceMethodsResolver.GetAllMethods(typeof(T)));
            _functionCacheConfigurationFunctions = new Dictionary<MethodInfo, object>();
        }

        public T Build() => CachedInterfaceFactoryInternal.Build(_originalImpl, _functionCacheConfigurationFunctions);

        #region Async
        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, Task<TValue>>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerAsync_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerAsync_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_2Params_KeySelector<TParam1, TParam2, TValue>, ICachedFunctionConfigurationManagerAsync_2Params<TParam1, TParam2, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>, ICachedFunctionConfigurationManagerAsync_3Params<TParam1, TParam2, TParam3, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>, ICachedFunctionConfigurationManagerAsync_4Params<TParam1, TParam2, TParam3, TParam4, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>, ICachedFunctionConfigurationManagerAsync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>, ICachedFunctionConfigurationManagerAsync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>, ICachedFunctionConfigurationManagerAsync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>, ICachedFunctionConfigurationManagerAsync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion
        
        #region AsyncCanx
        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, CancellationToken, Task<TValue>>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, CancellationToken, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_2Params_KeySelector<TParam1, TParam2, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_2Params<TParam1, TParam2, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, CancellationToken, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_3Params<TParam1, TParam2, TParam3, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, Task<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerAsyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion

        #region Sync
        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, TValue>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerSync_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerSync_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSync_2Params_KeySelector<TParam1, TParam2, TValue>, ICachedFunctionConfigurationManagerSync_2Params<TParam1, TParam2, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSync_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>, ICachedFunctionConfigurationManagerSync_3Params<TParam1, TParam2, TParam3, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSync_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>, ICachedFunctionConfigurationManagerSync_4Params<TParam1, TParam2, TParam3, TParam4, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSync_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>, ICachedFunctionConfigurationManagerSync_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSync_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>, ICachedFunctionConfigurationManagerSync_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSync_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>, ICachedFunctionConfigurationManagerSync_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSync_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>, ICachedFunctionConfigurationManagerSync_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion
        
        #region SyncCanx
        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, CancellationToken, TValue>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, CancellationToken, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_2Params_KeySelector<TParam1, TParam2, TValue>, ICachedFunctionConfigurationManagerSyncCanx_2Params<TParam1, TParam2, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, CancellationToken, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>, ICachedFunctionConfigurationManagerSyncCanx_3Params<TParam1, TParam2, TParam3, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>, ICachedFunctionConfigurationManagerSyncCanx_4Params<TParam1, TParam2, TParam3, TParam4, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>, ICachedFunctionConfigurationManagerSyncCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>, ICachedFunctionConfigurationManagerSyncCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>, ICachedFunctionConfigurationManagerSyncCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, TValue>>> expression,
            Func<CachedFunctionConfigurationManagerSyncCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>, ICachedFunctionConfigurationManagerSyncCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion
        
        #region ValueTask
        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, ValueTask<TValue>>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerValueTask_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_2Params_KeySelector<TParam1, TParam2, TValue>, ICachedFunctionConfigurationManagerValueTask_2Params<TParam1, TParam2, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>, ICachedFunctionConfigurationManagerValueTask_3Params<TParam1, TParam2, TParam3, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>, ICachedFunctionConfigurationManagerValueTask_4Params<TParam1, TParam2, TParam3, TParam4, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>, ICachedFunctionConfigurationManagerValueTask_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>, ICachedFunctionConfigurationManagerValueTask_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>, ICachedFunctionConfigurationManagerValueTask_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTask_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>, ICachedFunctionConfigurationManagerValueTask_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion
        
        #region ValueTaskCanx
        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, CancellationToken, ValueTask<TValue>>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, CancellationToken, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_2Params_KeySelector<TParam1, TParam2, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_2Params<TParam1, TParam2, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, CancellationToken, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_3Params_KeySelector<TParam1, TParam2, TParam3, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_3Params<TParam1, TParam2, TParam3, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_4Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_4Params<TParam1, TParam2, TParam3, TParam4, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_5Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_5Params<TParam1, TParam2, TParam3, TParam4, TParam5, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, CancellationToken, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_6Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_6Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, CancellationToken, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_7Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_7Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>(
            Expression<Func<T, Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, CancellationToken, ValueTask<TValue>>>> expression,
            Func<CachedFunctionConfigurationManagerValueTaskCanx_8Params_KeySelector<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>, ICachedFunctionConfigurationManagerValueTaskCanx_8Params<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        #endregion
        
        private void AddConfigFunc<TFunc>(Expression<Func<T, TFunc>> expression, object configurationFunc)
        {
            var methodInfo = GetMethodInfo(expression);
            
            if (_functionCacheConfigurationFunctions.ContainsKey(methodInfo))
                throw new Exception($"Duplicate configuration for {methodInfo}");
            
            _functionCacheConfigurationFunctions.Add(methodInfo, configurationFunc);
        }
        
        private MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodCallObject = (ConstantExpression)methodCallExpression.Object;
            var methodInfo = (MethodInfo)methodCallObject.Value;
            
            if (!_allInterfaceMethods.Contains(methodInfo))
                throw new InvalidOperationException($"Expression:'{expression.Body} does not match any methods on {typeof(T).Name}");

            return methodInfo;
        }
    }
}