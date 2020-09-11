using AppSoftware.KatexSharpRunner.Threading;
using Jint;

namespace AppSoftware.KatexSharpRunner
{
    public class JintEngine : Lockable
    {
        public JintEngine(Engine engine)
        {
            this.Engine = engine;
        }

        public Engine Engine { get; }
    }
}
