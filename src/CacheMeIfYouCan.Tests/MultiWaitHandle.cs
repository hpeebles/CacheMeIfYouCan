using System.Threading;

namespace CacheMeIfYouCan.Tests
{
    public class MultiWaitHandle
    {
        private readonly ManualResetEventSlim _waitHandle;
        private int _remaining;

        public MultiWaitHandle(int count)
        {
            _waitHandle = new ManualResetEventSlim();
            _remaining = count;
        }

        public void Mark()
        {
            if (Interlocked.Decrement(ref _remaining) == 0)
                _waitHandle.Set();
        }

        public void Wait()
        {
            _waitHandle.Wait();
        }
    }
}