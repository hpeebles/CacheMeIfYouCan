using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Configuration.CachedObject;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Singleton factory for creating <see cref="ICachedObject{T}"/> instances
    /// </summary>
    public static class CachedObjectFactory
    {
        /// <summary>
        /// Entry point to configure and build an <see cref="ICachedObject{T}"/> instance
        /// </summary>
        /// <param name="getValueFunc">The function whose value will be cached and exposed via the
        /// <see cref="ICachedObject{T}"/>. This function will be periodically called in the background to update
        /// the cached value</param>
        /// <typeparam name="T">The type of the value to be exposed via the <see cref="ICachedObject{T}"/></typeparam>
        /// <returns>A <see cref="CachedObjectConfigurationManager_ConfigureFor{T}"/> instance which should be used to
        /// configure and build the <see cref="ICachedObject{T}"/></returns>
        public static CachedObjectConfigurationManager_ConfigureFor<T> ConfigureFor<T>(Func<T> getValueFunc)
        {
            return ConfigureFor(() => Task.FromResult(getValueFunc()));
        }

        /// <summary>
        /// Entry point to configure and build an <see cref="ICachedObject{T}"/> instance
        /// </summary>
        /// <param name="getValueFunc">The async function whose value will be cached and exposed via the
        /// <see cref="ICachedObject{T}"/>. This function will be periodically called in the background to update
        /// the cached value</param>
        /// <typeparam name="T">The type of the value to be exposed via the <see cref="ICachedObject{T}"/></typeparam>
        /// <returns>A <see cref="CachedObjectConfigurationManager_ConfigureFor{T}"/> instance which should be used to
        /// configure and build the <see cref="ICachedObject{T}"/></returns>
        public static CachedObjectConfigurationManager_ConfigureFor<T> ConfigureFor<T>(Func<Task<T>> getValueFunc)
        {
            return new CachedObjectConfigurationManager_ConfigureFor<T>(getValueFunc);
        }
    }
}