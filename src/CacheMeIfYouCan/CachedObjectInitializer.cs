using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public static class CachedObjectInitializer
    {
        private static readonly Dictionary<Type, List<ICachedObjectInitializer>> Initializers;
        private static object Lock = new Object();

        static CachedObjectInitializer()
        {
            Initializers = new Dictionary<Type, List<ICachedObjectInitializer>>();
        }
        
        public static async Task<CachedObjectInitializeManyResult> InitializeAll()
        {
            ICachedObjectInitializer[] initializers;
            
            lock (Lock)
            {
                initializers = Initializers
                    .Values
                    .SelectMany(v => v)
                    .ToArray();
            }

            var tasks = initializers
                .Select(InitializeImpl)
                .ToArray();

            await Task.WhenAll(tasks);

            var results = tasks
                .Select(t => t.Result)
                .ToArray();
            
            return new CachedObjectInitializeManyResult(results);
        }

        public static async Task<CachedObjectInitializeManyResult> Initialize<T>()
        {
            List<ICachedObjectInitializer> initializers;
            
            lock (Lock)
            {
                if (!Initializers.TryGetValue(typeof(T), out initializers))
                    return new CachedObjectInitializeManyResult();
            }

            var tasks = initializers
                .Select(InitializeImpl)
                .ToArray();

            await Task.WhenAll(tasks);

            var results = tasks
                .Select(t => t.Result)
                .ToArray();
            
            return new CachedObjectInitializeManyResult(results);
        }

        internal static void Add<T>(ICachedObject<T> cachedObject)
        {
            var type = typeof(T);
            
            lock (Lock)
            {
                if (!Initializers.TryGetValue(type, out var list))
                {
                    list = new List<ICachedObjectInitializer>();
                    
                    Initializers.Add(type, list);
                }
                
                list.Add(cachedObject);
            }
        }

        internal static void Remove<T>(ICachedObject<T> cachedObject)
        {
            var type = typeof(T);

            lock (Lock)
            {
                if (!Initializers.TryGetValue(type, out var list))
                    return;

                list.RemoveAll(x => ReferenceEquals(x, cachedObject));

                if (!list.Any())
                    Initializers.Remove(type);
            }
        }

        private static async Task<CachedObjectInitializeResult> InitializeImpl(ICachedObjectInitializer cachedObject)
        {
            var timer = Stopwatch.StartNew();

            var outcome = await cachedObject.Initialize();
            
            return new CachedObjectInitializeResult(
                cachedObject.GetType().GenericTypeArguments[0],
                outcome,
                timer.Elapsed);
        }
    }
}