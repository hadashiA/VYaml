using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class EnumFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize_AsString()
        {
            Assert.That(Serialize(SimpleEnum.C), Is.EqualTo("c"));
        }

        [Test]
        public void Serialize_WithEnumMember()
        {
            Assert.That(Serialize(EnumMemberLabeledEnum.C), Is.EqualTo("c-alias"));
        }

        [Test]
        public void Serialize_WithDataMember()
        {
            Assert.That(Serialize(DataMemberLabeledEnum.C), Is.EqualTo("c-alias"));
        }

        [Test]
        public void Serialize_NamingConventionOptions()
        {
            var options = new YamlSerializerOptions
            {
                NamingConvention = NamingConvention.UpperCamelCase
            };
            Assert.That(Serialize(SimpleEnum.A, options), Is.EqualTo("A"));
            Assert.That(Serialize(NamingConventionEnum.HogeFuga, options), Is.EqualTo("hoge_fuga"));
            Assert.That(Serialize(DataMemberLabeledEnum.C, options), Is.EqualTo("c-alias"));
        }

        [Test]
        public void Deserialize_AsString()
        {
            var result = Deserialize<SimpleEnum>("c");
            Assert.That(result, Is.EqualTo(SimpleEnum.C));
        }

        [Test]
        public void Deserialize_WithEnumMember()
        {
            var result = Deserialize<EnumMemberLabeledEnum>("c-alias");
            Assert.That(result, Is.EqualTo(EnumMemberLabeledEnum.C));
        }

        [Test]
        public void Deserialize_WithDataMember()
        {
            var result = Deserialize<DataMemberLabeledEnum>("c-alias");
            Assert.That(result, Is.EqualTo(DataMemberLabeledEnum.C));
        }

        [Test]
        public void Deserialize_NamingConventionOptions()
        {
            var options = new YamlSerializerOptions
            {
                NamingConvention = NamingConvention.UpperCamelCase
            };
            Assert.That(Deserialize<SimpleEnum>("A", options), Is.EqualTo(SimpleEnum.A));
            Assert.That(Deserialize<NamingConventionEnum>("hoge_fuga", options), Is.EqualTo(NamingConventionEnum.HogeFuga));
            Assert.That(Deserialize<DataMemberLabeledEnum>("c-alias", options), Is.EqualTo(DataMemberLabeledEnum.C));
        }

        // --- [Flags] enum support (issue #179) ---

        [Test]
        public void Serialize_Flags_SingleValue()
        {
            Assert.That(Serialize(FlagsEnum.Read), Is.EqualTo("read"));
            Assert.That(Serialize(FlagsEnum.None), Is.EqualTo("none"));
        }

        [Test]
        public void Serialize_Flags_CompositeWithoutName()
        {
            // The exact scenario from issue #179: a combined value with no single matching name.
            // Members are joined with " | ", which stays a plain (unquoted) scalar.
            Assert.That(Serialize(FlagsEnum.Read | FlagsEnum.Write), Is.EqualTo("read | write"));
            Assert.That(Serialize(FlagsEnum.Read | FlagsEnum.Execute), Is.EqualTo("read | execute"));
        }

        [Test]
        public void Serialize_Flags_PrefersNamedCompositeMember()
        {
            // All == Read | Write | Execute is a defined member, so its own name wins.
            Assert.That(Serialize(FlagsEnum.Read | FlagsEnum.Write | FlagsEnum.Execute), Is.EqualTo("all"));
        }

        [Test]
        public void Deserialize_Flags_CommaSeparated()
        {
            Assert.That(Deserialize<FlagsEnum>("read, write"), Is.EqualTo(FlagsEnum.Read | FlagsEnum.Write));
        }

        [Test]
        public void Deserialize_Flags_PipeAndSpaceVariants()
        {
            Assert.That(Deserialize<FlagsEnum>("read | write"), Is.EqualTo(FlagsEnum.Read | FlagsEnum.Write));
            Assert.That(Deserialize<FlagsEnum>("read,execute"), Is.EqualTo(FlagsEnum.Read | FlagsEnum.Execute));
        }

        [Test]
        public void RoundTrip_Flags_Composite()
        {
            var value = FlagsEnum.Read | FlagsEnum.Execute;
            Assert.That(Deserialize<FlagsEnum>(Serialize(value)), Is.EqualTo(value));
        }

        [Test]
        public void Flags_RespectsAliases()
        {
            var value = FlagsAliasEnum.Read | FlagsAliasEnum.Write;
            Assert.That(Serialize(value), Is.EqualTo("r | w"));
            Assert.That(Deserialize<FlagsAliasEnum>("r | w"), Is.EqualTo(value));
        }

        [Test]
        public void Flags_RespectsNamingConvention()
        {
            var value = FlagsNamingConventionEnum.ReadOnly | FlagsNamingConventionEnum.WriteOnly;
            Assert.That(Serialize(value), Is.EqualTo("read_only | write_only"));
            Assert.That(Deserialize<FlagsNamingConventionEnum>(Serialize(value)), Is.EqualTo(value));
        }

        // A name that is plain-safe takes the cached-UTF8 fast path; a name that needs
        // quoting must fall back to the analyzing writer and still be quoted/escaped.
        [Test]
        public void Serialize_QuoteRequiringAlias_IsQuoted()
        {
            Assert.That(Serialize(QuoteRequiringAliasEnum.Yes), Is.EqualTo("\"true\""));
            Assert.That(Serialize(QuoteRequiringAliasEnum.Pair), Is.EqualTo("\"a: b\""));
        }

        [Test]
        public void RoundTrip_QuoteRequiringAlias()
        {
            foreach (var value in new[] { QuoteRequiringAliasEnum.Yes, QuoteRequiringAliasEnum.Pair })
            {
                Assert.That(Deserialize<QuoteRequiringAliasEnum>(Serialize(value)), Is.EqualTo(value));
            }
        }
    }
}
