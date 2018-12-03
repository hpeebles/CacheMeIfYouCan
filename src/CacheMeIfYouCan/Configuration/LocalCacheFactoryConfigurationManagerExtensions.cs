using System;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public static class LocalCacheFactoryConfigurationManagerExtensions
    {
        public static LocalCacheFactoryConfigurationManager<TK, TV> OnGetResultObservable<TK, TV>(
            this LocalCacheFactoryConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnGetResult, ordering);
        }
        
        public static LocalCacheFactoryConfigurationManager<TK, TV> OnSetResultObservable<TK, TV>(
            this LocalCacheFactoryConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnSetResult, ordering);
        }
        
        public static LocalCacheFactoryConfigurationManager<TK, TV> OnErrorObservable<TK, TV>(
            this LocalCacheFactoryConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
        
        public static LocalCacheFactoryConfigurationManager OnGetResultObservable(
            this LocalCacheFactoryConfigurationManager configManager,
            Action<IObservable<CacheGetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnGetResult, ordering);
        }
        
        public static LocalCacheFactoryConfigurationManager OnSetResultObservable(
            this LocalCacheFactoryConfigurationManager configManager,
            Action<IObservable<CacheSetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnSetResult, ordering);
        }
        
        public static LocalCacheFactoryConfigurationManager OnErrorObservable(
            this LocalCacheFactoryConfigurationManager configManager,
            Action<IObservable<CacheException>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
    }
}