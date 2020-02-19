using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
            var singleKeyAsync0ConfigAction = (Action<ICachedFunctionConfigurationManagerAsync_1Param_KeySelector<string, string>>)config[methods[0]];
            singleKeyAsync0ConfigAction(singleKeyAsync0ConfigManager);
            _singleKeyAsync0 = singleKeyAsync0ConfigManager.Build();

            var singleKeySync1ConfigManager = new CachedFunctionConfigurationManagerSync_1Param<string, string>(originalImpl.SingleKeySync);
            var singleKeySync1ConfigAction = (Action<ICachedFunctionConfigurationManagerSync_1Param_KeySelector<string, string>>)config[methods[1]];
            singleKeySync1ConfigAction(singleKeySync1ConfigManager);
            _singleKeySync1 = singleKeySync1ConfigManager.Build();
            
            var singleKeyAsyncCanx2ConfigManager = new CachedFunctionConfigurationManagerAsyncCanx_1Param<string, string>(originalImpl.SingleKeyAsyncCanx);
            var singleKeyAsyncCanx2ConfigAction = (Action<ICachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<string, string>>)config[methods[2]];
            singleKeyAsyncCanx2ConfigAction(singleKeyAsyncCanx2ConfigManager);
            _singleKeyAsyncCanx2 = singleKeyAsyncCanx2ConfigManager.Build();
            
            var singleKeySyncCanx3ConfigManager = new CachedFunctionConfigurationManagerSyncCanx_1Param<string, string>(originalImpl.SingleKeySyncCanx);
            var singleKeySyncCanx3ConfigAction = (Action<ICachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<string, string>>)config[methods[3]];
            singleKeySyncCanx3ConfigAction(singleKeySyncCanx3ConfigManager);
            _singleKeySyncCanx3 = singleKeySyncCanx3ConfigManager.Build();
            
            var enumerableKeyAsync4ConfigManager = new Configuration.EnumerableKeys.CachedFunctionConfigurationManagerAsync<IEnumerable<string>, Dictionary<string, string>, string, string>(originalImpl.EnumerableKeyAsync);
            var enumerableKeyAsync4ConfigAction = (Action<Configuration.EnumerableKeys.CachedFunctionConfigurationManagerAsync<IEnumerable<string>, Dictionary<string, string>, string, string>>)config[methods[4]];
            enumerableKeyAsync4ConfigAction(enumerableKeyAsync4ConfigManager);
            _enumerableKeyAsync4 = enumerableKeyAsync4ConfigManager.Build();
            
            var enumerableKeySync5ConfigManager = new Configuration.EnumerableKeys.CachedFunctionConfigurationManagerSync<IEnumerable<string>, Dictionary<string, string>, string, string>(originalImpl.EnumerableKeySync);
            var enumerableKeySync5ConfigAction = (Action<Configuration.EnumerableKeys.CachedFunctionConfigurationManagerSync<IEnumerable<string>, Dictionary<string, string>, string, string>>)config[methods[5]];
            enumerableKeySync5ConfigAction(enumerableKeySync5ConfigManager);
            _enumerableKeySync5 = enumerableKeySync5ConfigManager.Build();
            
            var enumerableKeyAsyncCanx6ConfigManager = new Configuration.EnumerableKeys.CachedFunctionConfigurationManagerAsyncCanx<IEnumerable<string>, Dictionary<string, string>, string, string>(originalImpl.EnumerableKeyAsyncCanx);
            var enumerableKeyAsyncCanx6ConfigAction = (Action<Configuration.EnumerableKeys.CachedFunctionConfigurationManagerAsyncCanx<IEnumerable<string>, Dictionary<string, string>, string, string>>)config[methods[6]];
            enumerableKeyAsyncCanx6ConfigAction(enumerableKeyAsyncCanx6ConfigManager);
            _enumerableKeyAsyncCanx6 = enumerableKeyAsyncCanx6ConfigManager.Build();
            
            var enumerableKeySyncCanx7ConfigManager = new Configuration.EnumerableKeys.CachedFunctionConfigurationManagerSyncCanx<IEnumerable<string>, Dictionary<string, string>, string, string>(originalImpl.EnumerableKeySyncCanx);
            var enumerableKeySyncCanx7ConfigAction = (Action<Configuration.EnumerableKeys.CachedFunctionConfigurationManagerSyncCanx<IEnumerable<string>, Dictionary<string, string>, string, string>>)config[methods[7]];
            enumerableKeySyncCanx7ConfigAction(enumerableKeySyncCanx7ConfigManager);
            _enumerableKeySyncCanx7 = enumerableKeySyncCanx7ConfigManager.Build();
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