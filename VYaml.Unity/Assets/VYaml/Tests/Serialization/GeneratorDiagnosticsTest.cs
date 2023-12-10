using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit;
using NUnit.Framework;
using VYaml.SourceGenerator;

namespace VYaml.Tests
{

    public class GeneratorDIagnosticsTest
    {
        [Test]
        public void MustBePartial()
        {
            var runResult = Generate(@"
[YamlObject]
class Hoge {}
");

            var generatedFileSyntax = runResult.GeneratedTrees.Single(t => t.FilePath.EndsWith("Vector3.g.cs"));
        }

        static GeneratorDriverRunResult Generate(string code)
        {
            // Create an instance of the source generator.
            var generator = new VYamlIncrementalSourceGenerator();

            // Source generators should be tested using 'GeneratorDriver'.
            var driver = CSharpGeneratorDriver.Create(generator);

            // We need to create a compilation with the required source code.
            var compilation = CSharpCompilation.Create(nameof(GeneratorDIagnosticsTest),
                new[] { CSharpSyntaxTree.ParseText(code) },
                new[]
                {
                    // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                });

            // Run generators and retrieve all results.
            return driver.RunGenerators(compilation).GetRunResult();
        }
    }
}
