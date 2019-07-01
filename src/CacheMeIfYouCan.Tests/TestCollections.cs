using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public static class TestCollections
    {
        public const string Cache = "Cache";
        public const string CachedObject = "CachedObject";
        public const string FunctionCache = "FunctionCache";
        public const string Proxy = "Proxy";
    }

    [CollectionDefinition(TestCollections.Cache)]
    public class CacheCollection : ICollectionFixture<CacheSetupLock>
    { }
    
    [CollectionDefinition(TestCollections.CachedObject)]
    public class CachedObjectCollection : ICollectionFixture<CachedObjectSetupLock>
    { }
    
    [CollectionDefinition(TestCollections.FunctionCache)]
    public class FunctionCacheCollection : ICollectionFixture<CacheSetupLock>
    { }
    
    [CollectionDefinition(TestCollections.Proxy)]
    public class ProxyCollection : ICollectionFixture<CacheSetupLock>
    { }
}