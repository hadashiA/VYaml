#if NET7_0_OR_GREATER
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using VYaml.Annotations;
using VYaml.SourceGenerator;

namespace VYaml.Tests
{
    [TestFixture]
    public class GeneratorDiagnosticsTest
    {
        static Compilation baseCompilation = default!;

        static Compilation CreateCompilation(string source)
        {
            return CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(YamlObjectUnionAttribute).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }

        [Test]
        [Ignore("Not passed")]
        public void MuestBePartial()
        {
            var code = "using VYaml.Annotations\n" +
                       "\n" +
                       "[YamlObject]\n" +
                       "public class Hoge { }";

            var inputCompilation = CreateCompilation(code);

            var driver = CSharpGeneratorDriver
                .Create(new VYamlSourceGenerator())
                .RunGeneratorsAndUpdateCompilation(
                    inputCompilation,
                    out var outputCompilation,
                    out var diagnostics);

            Assert.That(diagnostics.Length, Is.EqualTo(1));
        }
     }
}
#endif