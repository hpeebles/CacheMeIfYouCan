using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class TestImpl : ITest
    {
        public Task<string> StringToString(string key)
        {
            return Task.FromResult(key);
        }

        public Task<string> IntToString(int key)
        {
            return Task.FromResult(key.ToString());
        }

        public Task<double> DoubleToDouble(double key)
        {
            return Task.FromResult(key * 2);
        }
    }
}