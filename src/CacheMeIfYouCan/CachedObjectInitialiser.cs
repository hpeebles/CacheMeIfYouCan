using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public static class CachedObjectInitialiser
    {
        private static readonly ConcurrentDictionary<Type, Func<Task<bool>>> Initialisers = new ConcurrentDictionary<Type, Func<Task<bool>>>();
        
        public static async Task<bool> InitAll()
        {
            var tasks = Initialisers
                .Select(kv => kv.Value())
                .ToArray();

            await Task.WhenAll(tasks);

            return tasks.All(t => t.Result);
        }

        internal static void Register<T>(ICachedObject<T> cachedObject)
        {
            if (!Initialisers.TryAdd(typeof(T), cachedObject.Init))
                throw new Exception($"A CachedObject of type '{typeof(T).FullName}' already exists");
        }
    }
}