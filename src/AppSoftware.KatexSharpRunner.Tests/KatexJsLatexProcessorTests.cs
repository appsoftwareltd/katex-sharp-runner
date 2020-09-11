using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AppSoftware.KatexSharpRunner.Tests
{
    public class KatexJsLatexProcessorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private const string _skipSpeedTestReason = "SkipPerformanceTests";

        public KatexJsLatexProcessorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void RenderLatex()
        {
            // Seems to be some sort of problem running this in Rider - it just hangs

            var katexJsLatexProcessor = new KatexJsLatexProcessor();

            string html1 = katexJsLatexProcessor.RenderToString("c = \\pm\\sqrt{a^2 + b^2}");

            string html2 = katexJsLatexProcessor.RenderToString("c = \\pm\\sqrt{a^2 + b^2}");

            Assert.NotNull(html1);
            Assert.NotNull(html2);
        }

        [Fact(Skip = _skipSpeedTestReason)]
        public void RenderLatexSpeedTest()
        {
            // Seems to be some sort of problem running this in Rider - it just hangs

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            int renders = 1000;

            var katexJsLatexProcessor = new KatexJsLatexProcessor();

            long initialisationMilliseconds = stopwatch.ElapsedMilliseconds;

            // katexScriptInitMilliseconds tests out at about 0.5 seconds - significant overhead

            _testOutputHelper.WriteLine($"{nameof(initialisationMilliseconds)}: {initialisationMilliseconds}");

            stopwatch.Restart();

            for (int i = 0; i < renders; i++)
            {
                _ = katexJsLatexProcessor.RenderToString("c = \\pm\\sqrt{a^2 + b^2}");
            }

            stopwatch.Stop();

            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            double millisecondsPerRender = elapsedMilliseconds / (double) renders;

            _testOutputHelper.WriteLine($"{nameof(millisecondsPerRender)}: {millisecondsPerRender}");

            // 0.281 seconds if creating new KatexScript per render but only 0.025 if reusing over tested over 10000 iterations.
            // So there is capacity for 40 renders / second on test hardware (Surface Book 2). With caching for server side rendering
            // this should be performant enough per server.

            Assert.True(millisecondsPerRender < 100);
        }

        [Fact(Skip = _skipSpeedTestReason)]
        public void RenderToStringSpeedTestMultiThreaded()
        {
            // Seems to be some sort of problem running this in Rider - it just hangs

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            int renders = 1000;

            // Optimal jintEngine count seems to be 4 on Surface Book 2

            var katexJsLatexProcessor = new KatexJsLatexProcessor(4, 2000);

            long initialisationMilliseconds = stopwatch.ElapsedMilliseconds;

            // initialisationMilliseconds tests out at about 500 ms - significant overhead

            _testOutputHelper.WriteLine($"{nameof(initialisationMilliseconds)}: {initialisationMilliseconds}");

            stopwatch.Restart();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Parallel.For(0, renders, options, async i =>
            {
                _ = katexJsLatexProcessor.RenderToString("c = \\pm\\sqrt{a^2 + b^2}");
            });

            stopwatch.Stop();

            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            double millisecondsPerRender = elapsedMilliseconds / (double)renders;

            _testOutputHelper.WriteLine($"{nameof(millisecondsPerRender)}: {millisecondsPerRender}");
            _testOutputHelper.WriteLine($"{stopwatch.ElapsedMilliseconds}: {elapsedMilliseconds}");

            Assert.True(millisecondsPerRender < 100);
        }

        [Fact(Skip = _skipSpeedTestReason)]
        public void RenderToStringAsyncSpeedTestMultiThreaded()
        {
            // Seems to be some sort of problem running this in Rider - it just hangs

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            int renders = 1000;

            // Optimal jintEngine count seems to be 4 on Surface Book 2

            var katexJsLatexProcessor = new KatexJsLatexProcessor(4, 2000);

            long initialisationMilliseconds = stopwatch.ElapsedMilliseconds;

            // initialisationMilliseconds tests out at about 500 ms - significant overhead

            _testOutputHelper.WriteLine($"{nameof(initialisationMilliseconds)}: {initialisationMilliseconds}");

            stopwatch.Restart();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Parallel.For(0, renders, options, async i =>
            {
                _ = await katexJsLatexProcessor.RenderToStringAsync("c = \\pm\\sqrt{a^2 + b^2}").ConfigureAwait(false);
            });

            stopwatch.Stop();

            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            double millisecondsPerRender = elapsedMilliseconds / (double)renders;

            _testOutputHelper.WriteLine($"{nameof(millisecondsPerRender)}: {millisecondsPerRender}");
            _testOutputHelper.WriteLine($"{stopwatch.ElapsedMilliseconds}: {elapsedMilliseconds}");

            Assert.True(millisecondsPerRender < 100);
        }

        [Fact(Skip = _skipSpeedTestReason)]
        public void RenderToStringSpeedTestMultiThreadedFlooded()
        {
            int renders = 1000;

            // Optimal jintEngine count seems to be 4 on Surface Book 2

            var katexJsLatexProcessor = new KatexJsLatexProcessor(1, 500);

            var options = new ParallelOptions { MaxDegreeOfParallelism = 100 };

            // Aggregate exception made up of timeouts

            Assert.Throws<AggregateException>(() =>
            {
                Parallel.For(0, renders, options, i =>
                {
                    _ = katexJsLatexProcessor.RenderToString("c = \\pm\\sqrt{a^2 + b^2}");
                });
            });
        }
    }
}