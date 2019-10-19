using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Singleton which holds all of the registered <see cref="ICachedObject{T}"/> instances and exposes the option to
    /// initialize them all.
    /// </summary>
    /// <remarks>When using <see cref="ICachedObject"/> instances in production it is recommended that you call
    /// <see cref="InitializeAll"/> during your service startup</remarks>
    public static class CachedObjectInitializer
    {
        private static readonly Dictionary<Type, List<ICachedObject>> CachedObjects;
        private static readonly object Lock = new Object();

        static CachedObjectInitializer()
        {
            CachedObjects = new Dictionary<Type, List<ICachedObject>>();
        }

        /// <summary>
        /// Initializes all instances of <see cref="ICachedObject"/>
        /// </summary>
        public static CachedObjectInitializeManyResult InitializeAll()
        {
            return Task.Run(InitializeAllAsync).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Initializes all instances of <see cref="ICachedObject"/>
        /// </summary>
        public static async Task<CachedObjectInitializeManyResult> InitializeAllAsync()
        {
            ICachedObject[] cachedObjects;
            var timer = Stopwatch.StartNew();
            
            lock (Lock)
            {
                cachedObjects = CachedObjects
                    .Values
                    .SelectMany(v => v)
                    .ToArray();
            }

            var tasks = cachedObjects
                .Select(InitializeImpl)
                .ToArray();

            await Task.WhenAll(tasks);

            var results = tasks
                .Select(t => t.Result)
                .ToArray();
            
            return new CachedObjectInitializeManyResult(results, timer.Elapsed);
        }

        internal static void Add<T>(ICachedObject<T> cachedObject)
        {
            var type = typeof(T);
            
            lock (Lock)
            {
                if (!CachedObjects.TryGetValue(type, out var list))
                {
                    list = new List<ICachedObject>();
                    
                    CachedObjects.Add(type, list);
                }
                
                list.Add(cachedObject);
            }
        }

        internal static void Remove<T>(ICachedObject<T> cachedObject)
        {
            var type = typeof(T);

            lock (Lock)
            {
                if (!CachedObjects.TryGetValue(type, out var list))
                    return;

                list.RemoveAll(x => ReferenceEquals(x, cachedObject));

                if (!list.Any())
                    CachedObjects.Remove(type);
            }
        }

        private static async Task<CachedObjectInitializeResult> InitializeImpl(ICachedObject cachedObject)
        {
            var timer = Stopwatch.StartNew();

            var outcome = await cachedObject.InitializeAsync();
            
            return new CachedObjectInitializeResult(
                cachedObject.Name,
                cachedObject.GetType().GenericTypeArguments[0],
                outcome,
                timer.Elapsed);
        }
    }
}