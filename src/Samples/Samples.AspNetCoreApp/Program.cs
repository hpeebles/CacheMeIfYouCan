using System;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Samples.AspNetCoreApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            await InitializeCachedObjects();

            await host.RunAsync();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static async Task InitializeCachedObjects()
        {
            // This initializes all instances of ICachedObject<> so that all
            // subsequent requests return objects that are already in memory
            var initAllResult = await CachedObjectInitializer.InitializeAll();
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
