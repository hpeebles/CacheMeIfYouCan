using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.SingleKey;

namespace CacheMeIfYouCan.Configuration
{
    public sealed partial class CachedInterfaceConfigurationManager<T>
    {
        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, Task<TValue>>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerAsync_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerAsync_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, CancellationToken, Task<TValue>>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerAsyncCanx_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }

        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, TValue>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerSync_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerSync_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
        
        public CachedInterfaceConfigurationManager<T> Configure<TKey, TValue>(
            Expression<Func<T, Func<TKey, CancellationToken, TValue>>> expression,
            Func<ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<TKey, TValue>, ICachedFunctionConfigurationManagerSyncCanx_1Param<TKey, TValue>> configurationFunc)
        {
            AddConfigFunc(expression, configurationFunc);
            return this;
        }
    }
}