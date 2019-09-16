using System;
using System.Linq;
using CacheMeIfYouCan;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Samples.AspNetCoreApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            InitializeCachedObjects();

            host.Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void InitializeCachedObjects()
        {
            // This initializes all instances of ICachedObject<> so that all
            // subsequent requests return objects that are already in memory
            var initAllResult = CachedObjectInitializer.InitializeAll();
            if (!initAllResult.Success)
            {
                var outcomes = initAllResult
                    .Results
                    .Select(r => $"Type: '{r.CachedObjectType.Name}' Outcome: '{r.Outcome}'")
                    .ToArray();

                throw new Exception(
                    "Failed to initialize all ICachedObject<>'s." +
                    Environment.NewLine +
                    String.Join(Environment.NewLine, outcomes));
            }
        }
    }
}
