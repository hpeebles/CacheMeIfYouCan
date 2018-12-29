namespace CacheMeIfYouCan.Redis.Tests
{
    public static class TestConnectionString
    {
        public static readonly string Value = $"redis-test:{Port},password=xxxxxxxx";

        private const int Port = 14456;
    }
}