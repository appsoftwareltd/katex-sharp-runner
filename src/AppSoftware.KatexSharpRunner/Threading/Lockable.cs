using System;
using System.Threading;

namespace AppSoftware.KatexSharpRunner.Threading
{
    public abstract class Lockable
    {
        protected Lockable()
        {
            this.SyncRoot = new object();
        }

        private object SyncRoot { get; }

        public LockHandle Lock(int timeoutMs)
        {
            if (Monitor.TryEnter(this.SyncRoot, timeoutMs))
            {
                return new LockHandle(this.SyncRoot);
            }

            throw new TimeoutException($"{this.GetType()} ({typeof(Lockable)}) {nameof(Lock)} failed to acquire the lock before timeout ({timeoutMs}ms).");
        }
    }
}
