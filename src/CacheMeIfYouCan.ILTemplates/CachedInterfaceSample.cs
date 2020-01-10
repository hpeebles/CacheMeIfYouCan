using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration.SingleKey;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan.ILTemplates
{
    public sealed class CachedInterfaceSample : ISampleInterface
    {
        private readonly Func<string, Task<string>> _singleKeyAsync0;
        private readonly Func<string, string> _singleKeySync1;
        private readonly Func<string, CancellationToken, Task<string>> _singleKeyAsyncCanx2;
        private readonly Func<string, CancellationToken, string> _singleKeySyncCanx3;
        
        public CachedInterfaceSample(ISampleInterface originalImpl, Dictionary<MethodInfo, object> config)
        {
            var methods = InterfaceMethodsResolver.GetAllMethods(typeof(ISampleInterface));

            var singleKeyAsync0ConfigManager = new CachedFunctionConfigurationManagerAsync<string, string>(originalImpl.SingleKeyAsync);
            var singleKeyAsync0ConfigAction = (Action<CachedFunctionConfigurationManagerAsync<string, string>>)config[methods[0]];
            singleKeyAsync0ConfigAction(singleKeyAsync0ConfigManager);
            _singleKeyAsync0 = singleKeyAsync0ConfigManager.Build();

            var singleKeySync1ConfigManager = new CachedFunctionConfigurationManagerSync<string, string>(originalImpl.SingleKeySync);
            var singleKeySync1ConfigAction = (Action<CachedFunctionConfigurationManagerSync<string, string>>)config[methods[1]];
            singleKeySync1ConfigAction(singleKeySync1ConfigManager);
            _singleKeySync1 = singleKeySync1ConfigManager.Build();
            
            var singleKeyAsyncCanx2ConfigManager = new CachedFunctionConfigurationManagerAsyncCanx<string, string>(originalImpl.SingleKeyAsyncCanx);
            var singleKeyAsyncCanx2ConfigAction = (Action<CachedFunctionConfigurationManagerAsyncCanx<string, string>>)config[methods[2]];
            singleKeyAsyncCanx2ConfigAction(singleKeyAsyncCanx2ConfigManager);
            _singleKeyAsyncCanx2 = singleKeyAsyncCanx2ConfigManager.Build();
            
            var singleKeySyncCanx3ConfigManager = new CachedFunctionConfigurationManagerSyncCanx<string, string>(originalImpl.SingleKeySyncCanx);
            var singleKeySyncCanx3ConfigAction = (Action<CachedFunctionConfigurationManagerSyncCanx<string, string>>)config[methods[3]];
            singleKeySyncCanx3ConfigAction(singleKeySyncCanx3ConfigManager);
            _singleKeySyncCanx3 = singleKeySyncCanx3ConfigManager.Build();
        }
        
        public Task<string> SingleKeyAsync(string key) => _singleKeyAsync0(key);
        public string SingleKeySync(string key) => _singleKeySync1(key);
        public Task<string> SingleKeyAsyncCanx(string key, CancellationToken cancellationToken) => _singleKeyAsyncCanx2(key, cancellationToken);
        public string SingleKeySyncCanx(string key, CancellationToken cancellationToken) => _singleKeySyncCanx3(key, cancellationToken);
    }
}