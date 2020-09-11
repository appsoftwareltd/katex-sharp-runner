using System.Threading;

namespace AppSoftware.KatexSharpRunner.Threading
{
    public class ThreadSafeRoundRobin
    {
        private readonly int _elementCount;
        private int _lastIndex;

        public ThreadSafeRoundRobin(int elementCount)
        {
            _elementCount = elementCount;

            _lastIndex = -1;
        }

        public int GetNextIndex()
        {
            int nextIndex = Interlocked.Increment(ref _lastIndex);

            int index = nextIndex % _elementCount;

            var resultIndex = index >= 0 ? index : -index;

            return resultIndex;
        }
    }
}
