// #if NET7_0_OR_GREATER
// using Microsoft.CodeAnalysis;
// using System;
// using System.Collections.Generic;
// using System.Collections.Immutable;
// using System.Linq;
// using System.Reflection;
// using System.Security.Cryptography.X509Certificates;
// using System.Text;
// using System.Threading.Tasks;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.Diagnostics;
// using NUnit.Framework;
// using VYaml.SourceGenerator;
//
// namespace VYaml.Tests
// {
//     [TestFixture]
//     public class GeneratorDiagnosticsTest
//     {
//         [Test]
//         public void MEMPACK001_MuestBePartial()
//         {
//
//             Compile(1, """
// using MemoryPack;
//
// [MemoryPackable]
// public class Hoge
// {
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK002_NestedNotAllow()
//         {
//             Compile(2, """
// using MemoryPack;
//
// public partial class Hoge
// {
//     [MemoryPackable]
//     public partial class Huga
//     {
//     }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK003_AbstractMustUnion()
//         {
//             Compile(3, """
// using MemoryPack;
//
// [MemoryPackable]
// public abstract partial class Hoge
// {
// }
// """);
//
//             Compile(3, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial interface IHoge
// {
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK004_MultipleCtorWithoutAttribute()
//         {
//             Compile(4, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     public Hoge()
//     {
//     }
//
//     public Hoge(int x)
//     {
//     }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK005_MultipleCtorAttribute()
//         {
//             Compile(5, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     [MemoryPackConstructor]
//     public Hoge()
//     {
//     }
//
//     [MemoryPackConstructor]
//     public Hoge(int x)
//     {
//     }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK006_ConstructorHasNoMatchedParameter()
//         {
//             Compile(6, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     public int Foo { get; set;}
//
//     [MemoryPackConstructor]
//     public Hoge(int hhogee)
//     {
//         this.Foo = hhogee;
//     }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK007_OnMethodHasParameter()
//         {
//             Compile(7, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     [MemoryPackOnSerializing]
//     void Foo(int x)
//     {
//     }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK008_OnMethodInUnamannagedType()
//         {
//             Compile(8, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial struct Hoge
// {
//     [MemoryPackOnSerializing]
//     void Foo()
//     {
//     }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK009_OverrideMemberCantAddAnnotation()
//         {
//             Compile(9, """
// using MemoryPack;
//
// public abstract class MyClass
// {
//     public abstract int MyProperty { get; set; }
// }
//
// [MemoryPackable]
// public partial class MyClass2 : MyClass
// {
//     [MemoryPackIgnore]
//     public override int MyProperty { get; set; }
// }
// """);
//
//             Compile(9, """
// using MemoryPack;
//
// public abstract class MyClass
// {
//     public abstract int MyProperty { get; set; }
// }
//
// [MemoryPackable]
// public partial class MyClass3 : MyClass
// {
//     [MemoryPackInclude]
//     public override int MyProperty { get; set; }
// }
//
// """);
//         }
//
//         [Fact]
//         public void MEMPACK010_016_Union()
//         {
//             Compile(10, """
// using MemoryPack;
//
// [MemoryPackable]
// [MemoryPackUnion(0, typeof(string))]
// public sealed partial class MyClass
// {
// }
// """, allowMultipleError: true);
//
//             Compile(11, """
// using MemoryPack;
//
// [MemoryPackable]
// [MemoryPackUnion(0, typeof(string))]
// public partial class MyClass
// {
// }
// """, allowMultipleError: true);
//
//             Compile(12, """
// using MemoryPack;
//
// [MemoryPackable]
// [MemoryPackUnion(1, typeof(MyClass1))]
// [MemoryPackUnion(1, typeof(MyClass2))]
// public partial interface IMyClass
// {
// }
//
// [MemoryPackable]
// public partial class MyClass1 : IMyClass
// {
// }
//
// [MemoryPackable]
// public partial class MyClass2 : IMyClass
// {
// }
// """, allowMultipleError: true);
//
//             Compile(13, """
// using MemoryPack;
//
// [MemoryPackable]
// [MemoryPackUnion(1, typeof(MyClass1))]
// [MemoryPackUnion(2, typeof(MyClass2))]
// public partial interface IMyClass
// {
// }
//
// [MemoryPackable]
// public partial class MyClass1 : IMyClass
// {
// }
//
// [MemoryPackable]
// public partial class MyClass2
// {
// }
// """, allowMultipleError: true);
//
//             Compile(14, """
// using MemoryPack;
//
// [MemoryPackable]
// [MemoryPackUnion(1, typeof(MyClass1))]
// [MemoryPackUnion(2, typeof(MyClass2))]
// public abstract partial class MyClassBase
// {
// }
//
// [MemoryPackable]
// public partial class MyClass1 : MyClassBase
// {
// }
//
// [MemoryPackable]
// public partial class MyClass2
// {
// }
// """, allowMultipleError: true);
//
//             Compile(15, """
// using MemoryPack;
//
// [MemoryPackable]
// [MemoryPackUnion(1, typeof(MyClass1))]
// [MemoryPackUnion(2, typeof(MyClass2))]
// public partial interface IMyClass
// {
// }
//
// [MemoryPackable]
// public partial class MyClass1 : IMyClass
// {
// }
//
// [MemoryPackable]
// public partial struct MyClass2 : IMyClass
// {
// }
// """, allowMultipleError: true);
//
//             Compile(16, """
// using MemoryPack;
//
// [MemoryPackable]
// [MemoryPackUnion(1, typeof(MyClass1))]
// [MemoryPackUnion(2, typeof(MyClass2))]
// public partial interface IMyClass
// {
// }
//
// [MemoryPackable]
// public partial class MyClass1 : IMyClass
// {
// }
//
// // [MemoryPackable]
// public partial class MyClass2 : IMyClass
// {
// }
// """, allowMultipleError: true);
//
//         }
//
//
//
//         [Fact]
//         public void MEMPACK018_MemberCantSerializeType()
//         {
//             Compile(18, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     public object Foo { get; set;}
// }
// """);
//
//             Compile(18, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     public System.Array Foo { get; set;}
// }
// """);
//
//             Compile(18, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     public System.Action Foo { get; set;}
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK019_MemberIsNotMemoryPackable()
//         {
//             Compile(19, """
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     public Foo Bar { get; set;}
// }
//
// public class Foo { }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK020_TypeIsRefStruct()
//         {
//             Compile(20, """
// using MemoryPack;
//
// [MemoryPackable]
// public ref partial struct Hoge
// {
//     public int Bar { get; set;}
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK021_MemberIsRefStruct()
//         {
//             Compile(21, """
// using System;
// using MemoryPack;
//
// [MemoryPackable]
// public partial class Hoge
// {
//     byte[] b = default!;
//     public ReadOnlySpan<byte> SpanProp => b;
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK022_CollectionGenerateIsAbstract()
//         {
//             Compile(22, """
// using System.Collections.Generic;
// using MemoryPack;
//
// [MemoryPackable(GenerateType.Collection)]
// public abstract partial class MyList : List<int>
// {
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK023_CollectionGenerateNotImplementedInterface()
//         {
//             Compile(23, """
// using MemoryPack;
//
// [MemoryPackable(GenerateType.Collection)]
// public partial class Hoge
// {
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK024_CollectionGenerateNoParameterlessConstructor()
//         {
//             Compile(24, """
// using System.Collections.Generic;
// using MemoryPack;
//
// [MemoryPackable(GenerateType.Collection)]
// public partial class Hoge : List<int>
// {
//     public Hoge(int x)
//     {
//         Add(x);
//     }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK025_AllMembersMustAnnotateOrder()
//         {
//             Compile(25, """
// using MemoryPack;
//
// [MemoryPackable(SerializeLayout.Explicit)]
// public partial class Hoge
// {
//     [MemoryPackOrder(0)]
//     public int Prop1 { get; set; }
//     public int Prop2 { get; set; }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK026_AllMembersMustBeContinuousNumber()
//         {
//             Compile(26, """
// using MemoryPack;
//
// [MemoryPackable(SerializeLayout.Explicit)]
// public partial class Hoge
// {
//     [MemoryPackOrder(0)]
//     public int Prop1 { get; set; }
//     [MemoryPackOrder(2)]
//     public int Prop2 { get; set; }
// }
// """);
//         }
//
//         [Fact]
//         public void MEMPACK033_CircularReferenceOnlyAllowsParameterlessConstructor()
//         {
//             Compile(33, """
// using MemoryPack;
//
// [MemoryPackable(GenerateType.CircularReference)]
// public partial class Hoge
// {
//     [MemoryPackOrder(0)]
//     public int Prop1 { get; set; }
//     [MemoryPackOrder(2)]
//     public int Prop2 { get; set; }
//
//     public Hoge(int prop1, int prop2)
//     {
//         this.Prop1 = prop1;
//         this.Prop2 = prop2;
//     }
// }
// """);
//         }
//
//         static Compilation baseCompilation = default!;
//
//         [ModuleInitializer]
//         public static void InitializeCompilation()
//         {
//             // running .NET Core system assemblies dir path
//             var baseAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
//             var systemAssemblies = Directory.GetFiles(baseAssemblyPath)
//                 .Where(x =>
//                 {
//                     var fileName = Path.GetFileName(x);
//                     if (fileName.EndsWith("Native.dll")) return false;
//                     return fileName.StartsWith("System") || (fileName is "mscorlib.dll" or "netstandard.dll");
//                 });
//
//             var references = systemAssemblies
//                 .Append(typeof(MemoryPackableAttribute).Assembly.Location) // System Assemblies + MemoryPack.Core.dll
//                 .Select(x => MetadataReference.CreateFromFile(x))
//                 .ToArray();
//
//             var compilation = CSharpCompilation.Create("generatortest",
//                 references: references,
//                 options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
//
//             baseCompilation = compilation;
//         }
//
//         static Diagnostic[] RunGenerator(string source, AnalyzerConfigOptionsProvider? options = null)
//         {
//             var driver = CSharpGeneratorDriver.Create(new VYamlSourceGenerator());
//             if (options != null)
//             {
//                 driver = (Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver)driver.WithUpdatedAnalyzerConfigOptions(options);
//             }
//
//             // var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp11))); // use C#11
//             var compilation = CSharpCompilation.Create("compilation", );
//
//             driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);
//
//             // combine diagnostics as result.(ignore warning)
//             var compilationDiagnostics = newCompilation.GetDiagnostics();
//             return diagnostics.Concat(compilationDiagnostics).Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
//         }
//     }
// }
// #endif