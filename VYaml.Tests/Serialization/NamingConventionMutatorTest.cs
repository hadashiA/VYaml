using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Serialization;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class NamingConventionMutatorTest
    {
        static readonly Encoding Utf8 = Encoding.UTF8;

        [Test]
        [TestCase("hadashiKickLand", NamingConvention.UpperCamelCase, "HadashiKickLand")]
        [TestCase("HadashiKickLand", NamingConvention.UpperCamelCase, "HadashiKickLand")]
        [TestCase("hadashi_kick_land", NamingConvention.UpperCamelCase, "HadashiKickLand")]
        [TestCase("hadashi-kick-land", NamingConvention.UpperCamelCase, "HadashiKickLand")]
        [TestCase("hadashiKickLand", NamingConvention.LowerCamelCase, "hadashiKickLand")]
        [TestCase("HadashiKickLand", NamingConvention.LowerCamelCase, "hadashiKickLand")]
        [TestCase("hadashi_kick_land", NamingConvention.LowerCamelCase, "hadashiKickLand")]
        [TestCase("hadashi-kick-land", NamingConvention.LowerCamelCase, "hadashiKickLand")]
        [TestCase("hadashiKickLand", NamingConvention.SnakeCase, "hadashi_kick_land")]
        [TestCase("HadashiKickLand", NamingConvention.SnakeCase, "hadashi_kick_land")]
        [TestCase("hadashi_kick_land", NamingConvention.SnakeCase, "hadashi_kick_land")]
        [TestCase("hadashi-kick-land", NamingConvention.SnakeCase, "hadashi_kick_land")]
        [TestCase("hadashiKickLand", NamingConvention.KebabCase, "hadashi-kick-land")]
        [TestCase("HadashiKickLand", NamingConvention.KebabCase, "hadashi-kick-land")]
        [TestCase("hadashi_kick_land", NamingConvention.KebabCase, "hadashi-kick-land")]
        [TestCase("hadashi-kick-land", NamingConvention.KebabCase, "hadashi-kick-land")]
        public void Mutate(string input, NamingConvention convention, string expected)
        {
            var mutator = NamingConventionMutator.Of(convention);
            var inputUtf8 = Utf8.GetBytes(input);

            Span<char> destination = stackalloc char[input.Length * 2];
            var success = mutator.TryMutate(input.AsSpan(), destination, out var written);
            Assert.That(success, Is.True);
            Assert.That(destination[..written].ToString(), Is.EqualTo(expected));

            Span<byte> destinationUtf8 = stackalloc byte[input.Length * 2];
            var successUtf8 = mutator.TryMutate(inputUtf8, destinationUtf8, out written);
            Assert.That(successUtf8, Is.True);
            Assert.That(Utf8.GetString(destinationUtf8[..written]), Is.EqualTo(expected));
        }
   

        [Test]
        [TestCase(NamingConvention.SnakeCase)]
        [TestCase(NamingConvention.LowerCamelCase)]
        public void Mutate_String(NamingConvention convention)
        {
            Assert.That(NamingConventionMutator.Mutate("HadashiKickLand", convention),
                Is.EqualTo(convention == NamingConvention.SnakeCase ? "hadashi_kick_land" : "hadashiKickLand"));

            // Large inputs fall back from stackalloc to a pooled buffer.
            var longInput = string.Concat(Enumerable.Repeat("FooBar", 100_000));
            var result = NamingConventionMutator.Mutate(longInput, convention);
            Assert.That(result.Length, Is.EqualTo(convention == NamingConvention.SnakeCase ? 800_000 - 1 : 600_000));
            Assert.That(result.IndexOf('\0'), Is.EqualTo(-1));

            Assert.That(NamingConventionMutator.Mutate("", convention), Is.EqualTo(""));
        }
    }
}
