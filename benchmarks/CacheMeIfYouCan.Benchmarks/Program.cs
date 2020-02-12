using System;
using System.Linq;

namespace CacheMeIfYouCan.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var benchmarkId = GetBenchmarkId(args);

            switch (benchmarkId)
            {
                case 1:
                    CachedFunctionWithEnumerableKeys.Run();
                    break;
                default:
                    CachedFunctionWithSingleKey.Run();
                    break;
            }
        }

        private static int GetBenchmarkId(string[] args)
        {
            if (args is null || !args.Any() || !Int32.TryParse(args[0], out var id))
                return 0;

            return id;
        }
    }
}