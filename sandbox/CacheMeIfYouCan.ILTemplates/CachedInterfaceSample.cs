using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Configuration.SingleKey;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.ILTemplates
{
    public sealed class CachedInterfaceSample : ISampleInterface
    {
        private readonly Func<string, Task<string>> _singleKeyAsync0;
        private readonly Func<string, string> _singleKeySync1;
        private readonly Func<string, CancellationToken, Task<string>> _singleKeyAsyncCanx2;
        private readonly Func<string, CancellationToken, string> _singleKeySyncCanx3;
        private readonly Func<IEnumerable<string>, Task<Dictionary<string, string>>> _enumerableKeyAsync4;
        private readonly Func<IEnumerable<string>, Dictionary<string, string>> _enumerableKeySync5;
        private readonly Func<IEnumerable<string>, CancellationToken, Task<Dictionary<string, string>>> _enumerableKeyAsyncCanx6;
        private readonly Func<IEnumerable<string>, CancellationToken, Dictionary<string, string>> _enumerableKeySyncCanx7;
        
        public CachedInterfaceSample(ISampleInterface originalImpl, Dictionary<MethodInfo, object> config)
        {
            var methods = InterfaceMethodsResolver.GetAllMethods(typeof(ISampleInterface));

            var singleKeyAsync0ConfigManager = new CachedFunctionConfigurationManagerAsync_1Param<string, string>(originalImpl.SingleKeyAsync);
            var singleKeyAsync0ConfigFunc = (Func<ISingleKeyCachedFunctionConfigurationManagerAsync_1Param_KeySelector<string, string>, ICachedFunctionConfigurationManagerAsync_1Param<string, string>>)config[methods[0]];
            _singleKeyAsync0 = singleKeyAsync0ConfigFunc(singleKeyAsync0ConfigManager).Build();

            var singleKeySync1ConfigManager = new CachedFunctionConfigurationManagerSync_1Param<string, string>(originalImpl.SingleKeySync);
            var singleKeySync1ConfigFunc = (Func<ISingleKeyCachedFunctionConfigurationManagerSync_1Param_KeySelector<string, string>, ICachedFunctionConfigurationManagerSync_1Param<string, string>>)config[methods[1]];
            _singleKeySync1 = singleKeySync1ConfigFunc(singleKeySync1ConfigManager).Build();
            
            var singleKeyAsyncCanx2ConfigManager = new CachedFunctionConfigurationManagerAsyncCanx_1Param<string, string>(originalImpl.SingleKeyAsyncCanx);
            var singleKeyAsyncCanx2ConfigFunc = (Func<ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<string, string>, ICachedFunctionConfigurationManagerAsyncCanx_1Param<string, string>>)config[methods[2]];
            _singleKeyAsyncCanx2 = singleKeyAsyncCanx2ConfigFunc(singleKeyAsyncCanx2ConfigManager).Build();
            
            var singleKeySyncCanx3ConfigManager = new CachedFunctionConfigurationManagerSyncCanx_1Param<string, string>(originalImpl.SingleKeySyncCanx);
            var singleKeySyncCanx3ConfigFunc = (Func<ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<string, string>, ICachedFunctionConfigurationManagerSyncCanx_1Param<string, string>>)config[methods[3]];
            _singleKeySyncCanx3 = singleKeySyncCanx3ConfigFunc(singleKeySyncCanx3ConfigManager).Build();
            
            var enumerableKeyAsync4ConfigManager = new Configuration.EnumerableKeys.CachedFunctionConfigurationManagerAsync<IEnumerable<string>, Dictionary<string, string>, string, string>(originalImpl.EnumerableKeyAsync);
            var enumerableKeyAsync4ConfigFunc = (Func<Configuration.EnumerableKeys.CachedFunctionConfigurationManagerAsync<IEnumerable<string>, Dictionary<string, string>, string, string>, ICachedFunctionConfigurationManagerAsync_1Param<IEnumerable<string>, Dictionary<string, string>>>)config[methods[4]];
            _enumerableKeyAsync4 = enumerableKeyAsync4ConfigFunc(enumerableKeyAsync4ConfigManager).Build();
            
            var enumerableKeySync5ConfigManager = new Configuration.EnumerableKeys.CachedFunctionConfigurationManagerSync<IEnumerable<string>, Dictionary<string, string>, string, string>(originalImpl.EnumerableKeySync);
            var enumerableKeySync5ConfigFunc = (Func<Configuration.EnumerableKeys.CachedFunctionConfigurationManagerSync<IEnumerable<string>, Dictionary<string, string>, string, string>, ICachedFunctionConfigurationManagerSync_1Param<IEnumerable<string>, Dictionary<string, string>>>)config[methods[5]];
            _enumerableKeySync5 = enumerableKeySync5ConfigFunc(enumerableKeySync5ConfigManager).Build();
            
            var enumerableKeyAsyncCanx6ConfigManager = new Configuration.EnumerableKeys.CachedFunctionConfigurationManagerAsyncCanx<IEnumerable<string>, Dictionary<string, string>, string, string>(originalImpl.EnumerableKeyAsyncCanx);
            var enumerableKeyAsyncCanx6ConfigFunc = (Func<Configuration.EnumerableKeys.CachedFunctionConfigurationManagerAsyncCanx<IEnumerable<string>, Dictionary<string, string>, string, string>, ICachedFunctionConfigurationManagerAsyncCanx_1Param<IEnumerable<string>, Dictionary<string, string>>>)config[methods[6]];
            _enumerableKeyAsyncCanx6 = enumerableKeyAsyncCanx6ConfigFunc(enumerableKeyAsyncCanx6ConfigManager).Build();
            
            var enumerableKeySyncCanx7ConfigManager = new Configuration.EnumerableKeys.CachedFunctionConfigurationManagerSyncCanx<IEnumerable<string>, Dictionary<string, string>, string, string>(originalImpl.EnumerableKeySyncCanx);
            var enumerableKeySyncCanx7ConfigFunc = (Func<Configuration.EnumerableKeys.CachedFunctionConfigurationManagerSyncCanx<IEnumerable<string>, Dictionary<string, string>, string, string>, ICachedFunctionConfigurationManagerSyncCanx_1Param<IEnumerable<string>, Dictionary<string, string>>>)config[methods[7]];
            _enumerableKeySyncCanx7 = enumerableKeySyncCanx7ConfigFunc(enumerableKeySyncCanx7ConfigManager).Build();
        }
        
        public Task<string> SingleKeyAsync(string key) => _singleKeyAsync0(key);
        public string SingleKeySync(string key) => _singleKeySync1(key);
        public Task<string> SingleKeyAsyncCanx(string key, CancellationToken cancellationToken) => _singleKeyAsyncCanx2(key, cancellationToken);
        public string SingleKeySyncCanx(string key, CancellationToken cancellationToken) => _singleKeySyncCanx3(key, cancellationToken);
        public Task<Dictionary<string, string>> EnumerableKeyAsync(IEnumerable<string> keys) => _enumerableKeyAsync4(keys);
        public Dictionary<string, string> EnumerableKeySync(IEnumerable<string> keys) => _enumerableKeySync5(keys);
        public Task<Dictionary<string, string>> EnumerableKeyAsyncCanx(IEnumerable<string> keys, CancellationToken cancellationToken) => _enumerableKeyAsyncCanx6(keys, cancellationToken);
        public Dictionary<string, string> EnumerableKeySyncCanx(IEnumerable<string> keys, CancellationToken cancellationToken) => _enumerableKeySyncCanx7(keys, cancellationToken);
    }
}