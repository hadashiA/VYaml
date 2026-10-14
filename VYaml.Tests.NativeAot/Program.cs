// Native AOT smoke test: publish with PublishAot and run. Every scenario here must work
// without the reflection-based registration path (module initializers run eagerly before
// Main on Native AOT). Exits non-zero on the first failure.
using System;
using System.Collections.Generic;
using System.Linq;
using VYaml.Annotations;
using VYaml.Serialization;
using VYaml.Tests.NativeAot;

// Root-only shapes (never a member of any [YamlObject]) must be declared explicitly:
[assembly: YamlSerializable(typeof(List<Config>))]
[assembly: YamlSerializable(typeof(Dictionary<string, Mode>))]
[assembly: YamlSerializable(typeof(GenericWrapper<double>))]

var failed = 0;

void Check(string label, bool condition)
{
    if (condition)
    {
        Console.WriteLine($"ok: {label}");
    }
    else
    {
        Console.Error.WriteLine($"FAILED: {label}");
        failed++;
    }
}

// 1. [YamlObject] with enum / collection / nested / generic members
var config = new Config
{
    Name = "vyaml",
    Port = 8080,
    Mode = Mode.Fast,
    Aliased = AliasedEnum.B,
    Tags = new List<string> { "a", "b" },
    Scores = new Dictionary<string, int> { { "x", 1 }, { "y", 2 } },
    Points = new[] { 1, 2, 3 },
    Child = new Child { Value = 42 },
    WrappedInt = new GenericWrapper<int> { Value = 100 },
    OptionalMode = Mode.Slow,
};

var yaml = YamlSerializer.SerializeToString(config);
Console.WriteLine("--- serialized ---");
Console.WriteLine(yaml);

var restored = YamlSerializer.Deserialize<Config>(YamlSerializer.Serialize(config));
Check("roundtrip: Name", restored.Name == "vyaml");
Check("roundtrip: Port", restored.Port == 8080);
Check("roundtrip: enum member", restored.Mode == Mode.Fast);
Check("roundtrip: enum alias ([EnumMember])", restored.Aliased == AliasedEnum.B && yaml.Contains("b-alias"));
Check("roundtrip: List<string>", restored.Tags is { Count: 2 });
Check("roundtrip: Dictionary<string, int>", restored.Scores is { Count: 2 } && restored.Scores["y"] == 2);
Check("roundtrip: int[]", restored.Points is { Length: 3 });
Check("roundtrip: nested [YamlObject]", restored.Child?.Value == 42);
Check("roundtrip: closed generic [YamlObject] member", restored.WrappedInt?.Value == 100);
Check("roundtrip: nullable enum", restored.OptionalMode == Mode.Slow);

// 2. interface union
IAnimal animal = new Cat { Meows = 3 };
var animalYaml = YamlSerializer.SerializeToString(animal);
Check("union: tag emitted", animalYaml.Contains("!cat"));
var restoredAnimal = YamlSerializer.Deserialize<IAnimal>(YamlSerializer.Serialize(animal));
Check("union: roundtrip", restoredAnimal is Cat { Meows: 3 });

// 3. abstract class union
AbstractBase abstractValue = new DerivedB { B = 7 };
var restoredAbstract = YamlSerializer.Deserialize<AbstractBase>(YamlSerializer.Serialize(abstractValue));
Check("abstract union: roundtrip", restoredAbstract is DerivedB { B: 7 });

// 4. [assembly: YamlSerializable] roots
var list = YamlSerializer.Deserialize<List<Config>>(
    YamlSerializer.Serialize(new List<Config> { config, config }));
Check("root: List<Config>", list is { Count: 2 } && list[1].Name == "vyaml");

var modeMap = YamlSerializer.Deserialize<Dictionary<string, Mode>>(
    YamlSerializer.Serialize(new Dictionary<string, Mode> { { "a", Mode.Fast }, { "b", Mode.Slow } }));
Check("root: Dictionary<string, Mode>", modeMap is { Count: 2 } && modeMap["b"] == Mode.Slow);

var wrapped = YamlSerializer.Deserialize<GenericWrapper<double>>(
    YamlSerializer.Serialize(new GenericWrapper<double> { Value = 1.5 }));
Check("root: GenericWrapper<double>", Math.Abs(wrapped.Value - 1.5) < double.Epsilon);

// 5. untyped parse (PrimitiveObjectFormatter path). The C# `dynamic` keyword itself is
// not AOT-friendly (runtime binder), so consume the object graph with casts instead.
var untyped = (Dictionary<object, object?>)YamlSerializer.Deserialize<object>(
    System.Text.Encoding.UTF8.GetBytes("foo: 100\nbar: baz\nitems: [1, 2]\n"))!;
Check("untyped: scalar", untyped["foo"] is int and 100);
Check("untyped: string", untyped["bar"] is "baz");
Check("untyped: sequence", untyped["items"] is List<object?> { Count: 2 });

// 6. multi document API
var documents = YamlSerializer.DeserializeMultipleDocuments<object>(
    System.Text.Encoding.UTF8.GetBytes("---\na: 1\n---\nb: 2\n"));
Check("multi document", documents.Count() == 2);

if (failed > 0)
{
    Console.Error.WriteLine($"{failed} check(s) failed");
    return 1;
}
Console.WriteLine("All Native AOT checks passed");
return 0;

namespace VYaml.Tests.NativeAot
{
    public enum Mode
    {
        Fast,
        Slow,
    }

    public enum AliasedEnum
    {
        [System.Runtime.Serialization.EnumMember(Value = "a-alias")]
        A,
        [System.Runtime.Serialization.EnumMember(Value = "b-alias")]
        B,
    }

    [YamlObject]
    public partial class Config
    {
        public string? Name { get; set; }
        public int Port { get; set; }
        public Mode Mode { get; set; }
        public AliasedEnum Aliased { get; set; }
        public List<string>? Tags { get; set; }
        public Dictionary<string, int>? Scores { get; set; }
        public int[]? Points { get; set; }
        public Child? Child { get; set; }
        public GenericWrapper<int>? WrappedInt { get; set; }
        public Mode? OptionalMode { get; set; }
    }

    [YamlObject]
    public partial class Child
    {
        public int Value { get; set; }
    }

    [YamlObject]
    public partial class GenericWrapper<T>
    {
        public T? Value { get; set; }
    }

    [YamlObject]
    [YamlObjectUnion("!cat", typeof(Cat))]
    [YamlObjectUnion("!dog", typeof(Dog))]
    public partial interface IAnimal
    {
    }

    [YamlObject]
    public partial class Cat : IAnimal
    {
        public int Meows { get; set; }
    }

    [YamlObject]
    public partial class Dog : IAnimal
    {
        public int Barks { get; set; }
    }

    [YamlObject]
    [YamlObjectUnion("!a", typeof(DerivedA))]
    [YamlObjectUnion("!b", typeof(DerivedB))]
    public abstract partial class AbstractBase
    {
    }

    [YamlObject]
    public partial class DerivedA : AbstractBase
    {
        public int A { get; set; }
    }

    [YamlObject]
    public partial class DerivedB : AbstractBase
    {
        public int B { get; set; }
    }
}
