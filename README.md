# KaTeX Sharp Runner

## A wrapper for the JavaScript LaTeX type setting library in C# / .NET

KaTeX sharp runner facilitates running of the KaTeX library `renderToString` function in .NET projects. It achieves this by wrapping the JS KaTeX library with the Jint JavaScript interpreter engine and managing thread safety.

## Implementation in .NET Projects

NuGet:

https://www.nuget.org/packages/AppSoftware.KatexSharpRunner/

```
dotnet add package AppSoftware.KatexSharpRunner --version 0.1.0
dotnet add package Jint -version 2.11.58
```

References to AppSoftware.KatexSharpRunner and the Jint JavaScript interpreter are required. 

```xml
<ItemGroup>
    <PackageReference Include="AppSoftware.KatexSharpRunner" Version="0.1.0" />
    <PackageReference Include="Jint" Version="2.11.58" />
</ItemGroup>
```

The KaTeX JS library is included in the AppSoftware.KatexSharpRunner NuGet package. For display in web browsers, the KaTeX stylesheet will need to be included manually.

```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/katex@0.12.0/dist/katex.min.css" integrity="sha384-AfEj0r4/OFrOo5t7NnNe46zW/tFgW6x/bCJG8FqQCEo3+Aro6EYUG4+cU+KJWu/X" crossorigin="anonymous">

```

## Try It Out

Katex Sharp Runner powers the LaTeX rendering on [AppSoftware Journals](https://journals.appsoftware.com/choose-plan) if you would like to try it out first.

![KaTeX rendering example on AppSoftware Journals](https://raw.githubusercontent.com/appsoftwareltd/katex-sharp-runner/master/resources/katex_render_example_as_journals.png)

## Sample Code

A sample project can be found at https://github.com/appsoftwareltd/katex-sharp-runner/tree/master/samples

**Note on performance:**

Using the async method overload `RenderToStringAsync`, each render looked to take around 10ms in testing
on a Surface Book 2 (1.9GHz Intel Core i7-8650U). This is after `KatexJsLatexProcessor` has been initialised
(which takes substantially longer, approx 500ms per jintEngineCount). For this reason a single instance
of KatexJsLatexProcessor is recommended per application instance. See comments below regarding thread safety.

```csharp
using System;
using AppSoftware.KatexSharpRunner;

namespace KatexSharpRunnerSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate KatexJsLatexProcessor. ILatexProcessor provides parsing for raw LaTeX strings.
            // ILatexProcessor can be used standalone or within IMarkdownLatexPreprocessor

            // The argument jintEngineCount causes KatexJsLatexProcessor
            // to create 4 Jint JavaScript Interpreter engine instances and load the KaTeX JS library into
            // each. This is because Jint is not thread safe so KatexJsLatexProcessor manages thread safety 
            // locking during function calls. Multiple instances allows for concurrent processing using a
            // round robin algorithm to distribute requests to each Jint engine.

            // Argument jintEngineLockTimeout specifies a timeout in milliseconds after which an exception will be thrown
            // if a lock cannot be obtained on an instance of the underlying Jint engine if the load on KatexJsLatexProcessor
            // is too high. You can set this value as high as needed.

            var latexProcessor = new KatexJsLatexProcessor(jintEngineCount: 4, jintEngineLockTimeout: 2000);

            // Instantiate MarkdownLatexPreprocessor. IMarkdownLatexPreprocessor provides parsing for LaTeX strings in markdown text.

            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(latexProcessor);

            // Services can be instantiated via DI as follows

            // services.AddSingleton<ILatexProcessor>(new KatexJsLatexProcessor(4, 2000));
            // services.AddSingleton<IMarkdownLatexPreprocessor, MarkdownLatexPreprocessor>();

            // Render a raw LaTeX expression. 

            string latexHtml = latexProcessor.RenderToStringAsync("f(a,b,c) = (a^2+b^2+c^2)^3").Result;

            Console.WriteLine("Latex expression output:");
            Console.WriteLine(latexHtml);

            string markdown = @"

## Example Markdown

This is an example markdown document

This is an expression in inline delmiters ...

$f(a,b,c) = (a^2+b^2+c^2)^3$

... and this one with block delimiters

$$

\sum_{i=1}^\infty\frac{1}{n^2}=\frac{\pi^2}{6}

$$

";

            // ... or use MarkdownLatexPreprocessor to process LaTeX expressions within
            // markdown text. 

            var markdownHtml = markdownLatexPreprocessor.ProcessLatex(markdown);

            Console.WriteLine("Markdown output:");
            Console.WriteLine(markdownHtml);
            Console.ReadLine();
        }
    }
}
```

## Client Side Support

An additional MarkdownLatexProcessor class is included for processing LaTeX expressions within markdown documents. The JavaScript version of the C# MarkdownLatexProcessor is included 

https://github.com/appsoftwareltd/katex-sharp-runner/blob/master/src/js/markdownLatexPreprocessor.js

Include KaTeX client scripts for display in web browsers. For more information see https://katex.org/docs/browser.html


```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/katex@0.12.0/dist/katex.min.css" integrity="sha384-AfEj0r4/OFrOo5t7NnNe46zW/tFgW6x/bCJG8FqQCEo3+Aro6EYUG4+cU+KJWu/X" crossorigin="anonymous">

<script defer src="https://cdn.jsdelivr.net/npm/katex@0.12.0/dist/katex.min.js" integrity="sha384-g7c+Jr9ZivxKLnZTDUhnkOnsh30B4H0rpLUpJ4jAIKs4fnJI+sEnkvrMWph2EDg4" crossorigin="anonymous"></script>

```

```js
/// MarkdownLatexPreprocessor preprocesses markdown to convert LaTeX expressions to HTML before handing off to a markdown rendering library.

let markdownLatexPreprocessor = new MarkdownLatexPreprocessor(katex);

markdown = markdownLatexPreprocessor.processLatex(markdown);

// Here we are using the markdown-it library to complete markdown rendering. https://github.com/markdown-it/markdown-it

markdown = markdownit.render(markdown);

```

## Licence

### Personal/Open-Source License

This library is made available under a GPLv3 license for open-source and personal projects.

GPLv3 license Summary:

The GPLv3 allows others to copy, distribute and modify the software as long as they state what has been changed when, and ensure that any modifications are also licensed under the GPL. Software incorporating (via compiler) GPL-licensed code must also be made also available under the GPLv3 along with build & install instructions.

### Commercial License

Please contact us at mail@appsoftware.com with your use case for pricing and licence information.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

### KaTeX

Please see https://github.com/KaTeX/KaTeX/blob/master/LICENSE for information regarding the KaTeX library which this project depends on. No relationsip with the KaTeX project is implied by it's use in this project.
