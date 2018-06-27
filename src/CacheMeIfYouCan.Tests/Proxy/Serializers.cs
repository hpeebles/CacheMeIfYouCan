using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class Serializers
    {
        [Fact]
        public async Task SetSerializer()
        {
            ITest impl = new TestImpl();

            var serializerA = new TestSerializer();
            var serializerB = new TestSerializer();

            var proxy = impl
                .Cached()
                .WithCacheFactory(new TestCacheFactory())
                .WithDefaultSerializer(serializerA)
                .WithSerializer<string>(serializerB)
                .Build();

            await proxy.DoubleToDouble(123);
            Assert.Equal(2, serializerA.SerializeCount);
            Assert.Equal(0, serializerA.DeserializeCount);
            
            serializerA.ResetCounts();
            
            for (var i = 1; i < 10; i++)
            {
                await proxy.DoubleToDouble(123);

                Assert.Equal(i, serializerA.SerializeCount);
                Assert.Equal(i, serializerA.DeserializeCount);
            }
            
            await proxy.StringToString("abc");
            Assert.Equal(2, serializerB.SerializeCount);
            Assert.Equal(0, serializerB.DeserializeCount);
            
            serializerB.ResetCounts();
            
            for (var i = 1; i < 10; i++)
            {
                await proxy.StringToString("abc");

                Assert.Equal(i, serializerB.SerializeCount);
                Assert.Equal(i, serializerB.DeserializeCount);
            }
        }
    }
}
