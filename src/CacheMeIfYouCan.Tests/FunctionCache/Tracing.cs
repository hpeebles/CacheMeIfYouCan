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
    public class Tracing
    {
        private readonly CacheSetupLock _setupLock;

        public Tracing(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task TraceContainerCapturesTraces()
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
            
            using (var traceContainer = CacheTraceContainer.Create())
            {
                var tasks = keys
                    .Select(cachedEcho)
                    .ToArray();

                await Task.WhenAll(tasks);

                traceContainer.Traces.Should().HaveCount(10);
                
                traceContainer.Traces
                    .Select(t => t.Result.Results.Single().KeyString)
                    .OrderBy(k => k)
                    .Should()
                    .BeEquivalentTo(keys);
            }
        }
     
        [Fact]
        public async Task ContainersOnlyCaptureTracesWithinTheirScope()
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
            
            using (var traceContainer1 = CacheTraceContainer.Create())
            {
                await cachedEcho("1only");

                using (var traceContainer2 = CacheTraceContainer.Create())
                {
                    await cachedEcho("both");

                    traceContainer1.Traces.Should().HaveCount(2);
                    traceContainer1.Traces.First().Result.Results.Single().KeyString.Should().Be("1only");
                    traceContainer1.Traces.Last().Result.Results.Single().KeyString.Should().Be("both");
                    
                    traceContainer2.Traces.Should().HaveCount(1);
                    traceContainer2.Traces.Single().Result.Results.Single().KeyString.Should().Be("both");
                }
            }
        }
    }
}
#endif