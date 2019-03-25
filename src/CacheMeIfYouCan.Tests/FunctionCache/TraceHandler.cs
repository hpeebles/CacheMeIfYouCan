#if ASYNCLOCAL
using System;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class TraceHandler
    {
        private readonly CacheSetupLock _setupLock;

        public TraceHandler(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task CapturesTraces()
        {
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .Build();
            }

            var keys = Enumerable
                .Range(0, 10)
                .Select(i => i.ToString())
                .ToArray();
            
            using (var traceHandler = CacheMeIfYouCan.TraceHandler.StartNew())
            {
                var tasks = keys
                    .Select(cachedEcho)
                    .ToArray();

                await Task.WhenAll(tasks);

                traceHandler.Traces.Should().HaveCount(10);
                
                traceHandler.Traces
                    .Select(t => t.Result.Results.Single().KeyString)
                    .OrderBy(k => k)
                    .Should()
                    .BeEquivalentTo(keys);
            }
        }
     
        [Fact]
        public async Task OnlyCapturesTracesWithinScope()
        {
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .Build();
            }

            await cachedEcho("none");
            
            using (var traceHandler1 = CacheMeIfYouCan.TraceHandler.StartNew())
            {
                await cachedEcho("1only");

                using (var traceHandler2 = CacheMeIfYouCan.TraceHandler.StartNew())
                {
                    await cachedEcho("both");

                    traceHandler1.Traces.Should().HaveCount(2);
                    traceHandler1.Traces.First().Result.Results.Single().KeyString.Should().Be("1only");
                    traceHandler1.Traces.Last().Result.Results.Single().KeyString.Should().Be("both");
                    
                    traceHandler2.Traces.Should().HaveCount(1);
                    traceHandler2.Traces.Single().Result.Results.Single().KeyString.Should().Be("both");
                }
            }
        }
        
        [Fact]
        public async Task OnDisposingActionIsExecuted()
        {
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .Build();
            }

            CacheTrace trace = null;
            using (CacheMeIfYouCan.TraceHandler.StartNew(t => trace = t.Trace))
                await cachedEcho("123");

            trace.Should().NotBeNull();
            trace.Result.Results.Single().KeyString.Should().Be("123");
        }
    }
}
#endif