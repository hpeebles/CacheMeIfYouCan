using System;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Observables
{
    public static class DistributedCacheFactoryConfigurationManagerExtensions
    {
        public static DistributedCacheFactoryConfigurationManager<TK, TV> OnGetResultObservable<TK, TV>(
            this DistributedCacheFactoryConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onResult, configManager.OnGetResult, ordering);
        }
        
        public static DistributedCacheFactoryConfigurationManager<TK, TV> OnSetResultObservable<TK, TV>(
            this DistributedCacheFactoryConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onResult, configManager.OnSetResult, ordering);
        }
        
        public static DistributedCacheFactoryConfigurationManager<TK, TV> OnErrorObservable<TK, TV>(
            this DistributedCacheFactoryConfigurationManager<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
        
        public static DistributedCacheFactoryConfigurationManager OnGetResultObservable(
            this DistributedCacheFactoryConfigurationManager configManager,
            Action<IObservable<CacheGetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onResult, configManager.OnGetResult, ordering);
        }
        
        public static DistributedCacheFactoryConfigurationManager OnSetResultObservable(
            this DistributedCacheFactoryConfigurationManager configManager,
            Action<IObservable<CacheSetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onResult, configManager.OnSetResult, ordering);
        }
        
        public static DistributedCacheFactoryConfigurationManager OnErrorObservable(
            this DistributedCacheFactoryConfigurationManager configManager,
            Action<IObservable<CacheException>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservableHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
    }
}