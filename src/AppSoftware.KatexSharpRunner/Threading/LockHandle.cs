using System;
using System.Threading;

namespace AppSoftware.KatexSharpRunner.Threading
{
    public class LockHandle : IDisposable
    {
        private readonly object _syncRoot;

        public LockHandle(object locker)
        {
            _syncRoot = locker;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Monitor.Exit(_syncRoot);
            }
        }
    }
}
