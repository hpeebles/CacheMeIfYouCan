
namespace CacheMeIfYouCan.Internal
{
    internal static class BatchingHelper
    {
        public static int[] GetBatchSizes(
            int valuesCount,
            int maxBatchSize,
            BatchBehaviour batchBehaviour)
        {
            var batchCount = ((valuesCount - 1) / maxBatchSize) + 1;
            var batchSizes = new int[batchCount];

            if (batchBehaviour == BatchBehaviour.FillBatchesEvenly)
            {
                var averageBatchSize = (double)valuesCount / batchCount;
                var totalAllocated = 0;
                for (var index = 0; index < batchCount - 1; index++)
                {
                    var nextBatchSize = (int)(averageBatchSize * (index + 1)) - totalAllocated;
                    batchSizes[index] = nextBatchSize;
                    totalAllocated += nextBatchSize;
                }

                var finalBatchSize = valuesCount - totalAllocated;
                batchSizes[batchCount - 1] = finalBatchSize;
            }
            else
            {
                for (var index = 0; index < batchCount - 1; index++)
                    batchSizes[index] = maxBatchSize;

                var finalBatchSize = valuesCount - ((batchCount - 1) * maxBatchSize);
                batchSizes[batchCount - 1] = finalBatchSize;
            }

            return batchSizes;
        }
    }
}