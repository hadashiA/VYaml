using System;
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
            var success = mutator.TryMutate(input, destination, out var written);
            Assert.That(success, Is.True);
            Assert.That(destination[..written].ToString(), Is.EqualTo(expected));

            Span<byte> destinationUtf8 = stackalloc byte[input.Length * 2];
            var successUtf8 = mutator.TryMutate(inputUtf8, destinationUtf8, out written);
            Assert.That(successUtf8, Is.True);
            Assert.That(Utf8.GetString(destinationUtf8[..written]), Is.EqualTo(expected));
        }
   }
}