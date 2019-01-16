using System;
using System.Linq;
using CacheMeIfYouCan;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Polly;
using CacheMeIfYouCan.Prometheus;
using CacheMeIfYouCan.Redis;
using CacheMeIfYouCan.Redis.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Swashbuckle.AspNetCore.Swagger;

namespace Samples.AspNetCoreApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var logger = new Logger();
            
            services.AddSingleton<ILogger>(logger);

            // Set up the default ICachedObject settings
            DefaultSettings
                .CachedObject
                .WithRefreshInterval(TimeSpan.FromMinutes(5))
                .OnException(ex => logger.LogError(ex, "CachedObject exception"))
                .WithJitterPercentage(10);
            
            // Set up the default cache settings
            DefaultSettings
                .Cache
                .WithTimeToLive(TimeSpan.FromMinutes(30))
                .WithMetrics()
                .WithLocalCacheFactory(new DictionaryCacheFactory())
                .WithRedis(
                    c => c.ConnectionString = TestConnectionString.Value,
                    c => c
                        .WithPolicy(Policy.Handle<Exception>().CircuitBreakerAsync(5, TimeSpan.FromSeconds(10)))
                        .SwallowExceptions());
            
            // Create and register an ICachedObject<ISet> which will refresh every minute
            services.AddSingleton<EnabledAffiliatesReader>();
            services.AddSingleton(p =>
            {
                var reader = p.GetService<EnabledAffiliatesReader>();
                return CachedObjectFactory
                    .ConfigureFor(() => reader.Get())
                    .WithRefreshInterval(TimeSpan.FromMinutes(1)) // override the default setting
                    .Build();
            });
            
            // Create and register a cached implementation of the IItemPriceReader interface
            services.AddSingleton<ItemPriceReader>();
            services.AddSingleton(p => p
                .GetRequiredService<ItemPriceReader>()
                .Cached<IItemPriceReader>()
                .Build());
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "CacheMeIfYouCan - Demo", Version = "v1" });
            });
            
            services.AddMvc();

            ConstructCachedObjects(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CacheMeIfYouCan - Demo V1");
                c.RoutePrefix = String.Empty;
            });
            app.UseMvc();
        }

        private static void ConstructCachedObjects(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            
            foreach (var service in services
                .Select(s => s.ServiceType)
                .Where(t => t.IsGenericType)
                .Where(t => t.GetGenericTypeDefinition() == typeof(ICachedObject<>)))
            {
                provider.GetService(service);
            }
        }
    }
}
