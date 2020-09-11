using System;
using System.Reflection;
using System.Threading.Tasks;
using AppSoftware.KatexSharpRunner.Interfaces;
using AppSoftware.KatexSharpRunner.Threading;
using Jint;
using Jint.Native;

namespace AppSoftware.KatexSharpRunner
{
    /// <summary>
    /// Underlying Jint Engine is not thread safe, but following constructor initialisation of
    /// Jint engine, usage is protected by locks to make methods on this class thread safe.
    /// Instaniation of Jint Engine with load of KaTeX library into the engine incurrs about 500 milliseconds.
    /// which is significant overhead. Following this individual renders of basic LaTeX strings take around 25 milliseconds.
    /// It is left up to the implementer whether to instantiate KatexScript as a singleton or create new each time and incur
    /// init overhead.
    /// </summary>
    public class KatexJsLatexProcessor : ILatexProcessor
    {
        private readonly int _jintEngineLockTimeout;
        private readonly JintEngine[] _jintEngines;

        private readonly ThreadSafeRoundRobin _threadSafeRoundRobin;

        /// <summary>
        /// Initializes a new instance of the <see cref="KatexJsLatexProcessor"/> class.
        /// Defaults to single JintEngine with lock timeout of 1000ms.
        /// </summary>
        public KatexJsLatexProcessor() : this(1, 1000)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KatexJsLatexProcessor"/> class.
        /// Configurable number of JintEngines and the timeout afterwhich attempt to aquire lock for each JintEngine.
        /// </summary>
        /// <param name="jintEngineCount"></param>
        /// <param name="jintEngineLockTimeout"></param>
        public KatexJsLatexProcessor(int jintEngineCount, int jintEngineLockTimeout)
        {
            _jintEngineLockTimeout = jintEngineLockTimeout;
            _jintEngines = new JintEngine[jintEngineCount];

            _threadSafeRoundRobin = new ThreadSafeRoundRobin(jintEngineCount);

            for (int i = 0; i < jintEngineCount; i++)
            {
                // Note that Jint beta version 2.11.58 failed to parse the katex script, reporting
                // an invalid regular expression that I could not find in source. Stable version
                // 2.11.58 works.

                var assembly = Assembly.GetExecutingAssembly();

                string katexLibScript = EmbeddedResourceService.GetResourceContent("katex.js", assembly);

                katexLibScript += Environment.NewLine;

                // Attempt to invoke katex.renderToString directly results in error indicating "Can only execute functions", so create
                // global function as a proxy and append to the script.

                katexLibScript += "function renderToStringProxy(latex) { return katex.renderToString(latex, { throwOnError: false }); };";

                // Load script

                var engine = new Engine().Execute(katexLibScript);

                _jintEngines[i] = new JintEngine(engine);
            }
        }

        /// <summary>
        /// Takes a raw LaTeX expression and returns HTML.
        /// </summary>
        /// <param name="latex"></param>
        /// <returns></returns>
        public async Task<string> RenderToStringAsync(string latex)
        {
            return await Task.Run(() => this.RenderToString(latex)).ConfigureAwait(false);
        }

        /// <summary>
        /// Takes a raw LaTeX expression and returns HTML.
        /// </summary>
        /// <param name="latex"></param>
        /// <returns></returns>
        public string RenderToString(string latex)
        {
            // Invoke the renderToStringProxy function

            JsValue result;

            // Threading expectations discussion.

            // https://gitter.im/sebastienros/jint?at=5dc34f90fb4dab784a7858f6
            // https://github.com/sebastienros/jint/issues/170

            var i = _threadSafeRoundRobin.GetNextIndex();

            var jintEngine = _jintEngines[i];

            using (_jintEngines[i].Lock(_jintEngineLockTimeout))
            {
                result = jintEngine.Engine.Invoke("renderToStringProxy", latex);
            }

            var html = result.ToString();

            return html;
        }

        // /// <summary>
        // /// Run sample from https://github.com/sebastienros/jint
        // /// </summary>
        // public void JintTest()
        // {
        //     // https://github.com/sebastienros/jint
        //
        //     var engine = new Engine().SetValue("log", new Action<object>(x => _logger.LogInformation(x.ToString())));
        //
        //     // Should see result in logs following running this method.
        //
        //     engine.Execute(@"
        //       function hello() {
        //         log('Hello World');
        //       };
        //
        //       hello();
        //     ");
        // }
    }
}