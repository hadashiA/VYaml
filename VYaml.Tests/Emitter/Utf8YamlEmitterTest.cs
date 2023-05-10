using System.Buffers;
using NUnit.Framework;
using VYaml.Emitter;
using VYaml.Internal;

namespace VYaml.Tests.Emitter
{
    [TestFixture]
    public class Utf8YamlEmitterTest
    {
        [Test]
        public void WriteNull()
        {
            using var emitter = CreateEmitter();
            emitter.WriteNull();
            Assert.That(ToString(in emitter), Is.EqualTo("null"));
        }

        [Test]
        [TestCase(true, ExpectedResult = "true")]
        [TestCase(false, ExpectedResult = "false")]
        public string WriteBool(bool value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteBool(value);
            return ToString(in emitter);
        }

        [Test]
        [TestCase(0, ExpectedResult = "0")]
        [TestCase(123, ExpectedResult = "123")]
        [TestCase(-123, ExpectedResult = "-123")]
        [TestCase(int.MaxValue, ExpectedResult = "2147483647")]
        [TestCase(int.MinValue, ExpectedResult = "-2147483648")]
        public string WriteInt32(int value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteInt32(value);
            return ToString(in emitter);
        }

        [Test]
        [TestCase(0, ExpectedResult = "0")]
        [TestCase(123, ExpectedResult = "123")]
        [TestCase(-123, ExpectedResult = "-123")]
        [TestCase(long.MaxValue, ExpectedResult = "9223372036854775807")]
        [TestCase(long.MinValue, ExpectedResult = "-9223372036854775808")]
        public string WriteInt64(long value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteInt64(value);
            return ToString(in emitter);
        }

        [Test]
        [TestCase(0u, ExpectedResult = "0")]
        [TestCase(123u, ExpectedResult = "123")]
        [TestCase(uint.MaxValue, ExpectedResult = "4294967295")]
        public string WriteUInt32(uint value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteUInt32(value);
            return ToString(in emitter);
        }

        [Test]
        [TestCase(0u, ExpectedResult = "0")]
        [TestCase(123u, ExpectedResult = "123")]
        [TestCase(ulong.MaxValue, ExpectedResult = "18446744073709551615")]
        public string WriteUInt64(ulong value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteUInt64(value);
            return ToString(in emitter);
        }

        [Test]
        [TestCase(0f, ExpectedResult = "0")]
        [TestCase(123.4567f, ExpectedResult = "123.4567")]
        [TestCase(-123.4567f, ExpectedResult = "-123.4567")]
        public string WriteFloat(float value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteFloat(value);
            return ToString(in emitter);
        }

        [Test]
        [TestCase(0.0, ExpectedResult = "0")]
        [TestCase(123.456789123, ExpectedResult = "123.456789123")]
        [TestCase(-123.456789123, ExpectedResult = "-123.456789123")]
        public string WriteDouble(double value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteDouble(value);
            return ToString(in emitter);
        }

        [Test]
        public void WriteString_PlainScalar()
        {
            using var emitter = CreateEmitter();
            emitter.WriteString("aiueo", ScalarStyle.Plain);
            Assert.That(ToString(in emitter), Is.EqualTo("aiueo"));
        }

        [Test]
        [TestCase("aaa\nbbb", ExpectedResult = "\"aaa\\nbbb\"")]
        [TestCase("aaa\tbbb", ExpectedResult = "\"aaa\\tbbb\"")]
        [TestCase("aaa\"bbb", ExpectedResult = "\"aaa\\\"bbb\"")]
        [TestCase("aaa'bbb", ExpectedResult = "\"aaa'bbb\"")]
        [TestCase("\0", ExpectedResult = "\"\\0\"")]
        [TestCase("\x8", ExpectedResult = "\"\\b\"")]
        [TestCase("\xA0", ExpectedResult = "\"\\_\"")]
        [TestCase("\x2028", ExpectedResult = "\"\\L\"")]
        [TestCase("\x1F", ExpectedResult = "\"\\u001f\"")]
        public string WriteString_DoubleQuotedScalar(string value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteString(value, ScalarStyle.DoubleQuoted);
            return ToString(in emitter);
        }

        [Test]
        [TestCase("aaa\nbbb", ExpectedResult = "'aaa\\nbbb'")]
        [TestCase("aaa\tbbb", ExpectedResult = "'aaa\\tbbb'")]
        [TestCase("aaa'bbb", ExpectedResult = "'aaa\\'bbb'")]
        [TestCase("\0", ExpectedResult = "'\\0'")]
        [TestCase("\x8", ExpectedResult = "'\\b'")]
        [TestCase("\xA0", ExpectedResult = "'\\_'")]
        [TestCase("\x2028", ExpectedResult = "'\\L'")]
        [TestCase("\x1F", ExpectedResult = "'\\u001f'")]
        public string WriteString_SingleQuotedScalar(string value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteString(value, ScalarStyle.SingleQuoted);
            return ToString(in emitter);
        }

        [Test]
        public void WriteString_LiteralScalar()
        {
            using var emitter = CreateEmitter();

            emitter.WriteString(
                "Mark McGwire's\nyear was crippled\nby a knee injury.\n",
                ScalarStyle.Literal);

            Assert.That(ToString(in emitter), Is.EqualTo(
                "|\n" +
                "  Mark McGwire's\n" +
                "  year was crippled\n" +
                "  by a knee injury.\n"
                ));
        }

        [Test]
        public void WriteString_LiteralScalar_NoNewLineAtEnd()
        {
            using var emitter = CreateEmitter();

            emitter.WriteString(
                "Mark McGwire's\nyear was crippled\nby a knee injury.",
                ScalarStyle.Literal);

            Assert.That(ToString(in emitter), Is.EqualTo(
                "|-\n" +
                "  Mark McGwire's\n" +
                "  year was crippled\n" +
                "  by a knee injury.\n"
            ));
        }

        [Test]
        public void WriteString_LiteralScalar_AllNewlinesFromEnd()
        {
            using var emitter = CreateEmitter();

            emitter.WriteString(
                "Mark McGwire's\nyear was crippled\nby a knee injury.\n\n\n",
                ScalarStyle.Literal);

            Assert.That(ToString(in emitter), Is.EqualTo(
                "|+\n" +
                "  Mark McGwire's\n" +
                "  year was crippled\n" +
                "  by a knee injury.\n" +
                "  \n" +
                "  \n"
            ));
        }

        [Test]
        public void WriteString_LiteralScalarInSequence()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            emitter.WriteString(
                "Mark McGwire's\nyear was crippled\nby a knee injury.\n",
                ScalarStyle.Literal);
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "- |\n" +
                "  Mark McGwire's\n" +
                "  year was crippled\n" +
                "  by a knee injury.\n"
            ));
        }

        [Test]
        public void WriteString_LiteralScalarInMapping()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            emitter.WriteString("aaa");
            emitter.WriteString(
                "Mark McGwire's\nyear was crippled\nby a knee injury.\n",
                ScalarStyle.Literal);
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "aaa: |\n" +
                "  Mark McGwire's\n" +
                "  year was crippled\n" +
                "  by a knee injury.\n"
            ));
        }

        [Test]
        public void WriteString_LiteralScalarNested()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            {
                emitter.BeginMapping();
                {
                    emitter.WriteString("aaa");
                    emitter.WriteString(
                        "Mark McGwire's\nyear was crippled\nby a knee injury.\n",
                        ScalarStyle.Literal);
                }
                emitter.EndMapping();
            }
            emitter.EndSequence();

            var result = ToString(in emitter);
            Assert.That(result, Is.EqualTo(
                "- aaa: |\n" +
                "    Mark McGwire's\n" +
                "    year was crippled\n" +
                "    by a knee injury.\n"
            ));
        }

        [Test]
        public void WriteString_AutoDetectPlainScalar()
        {
            using var emitter = CreateEmitter();
            emitter.WriteString("aiueo kakikukeko");
            Assert.That(ToString(in emitter), Is.EqualTo("aiueo kakikukeko"));
        }

        [Test]
        [TestCase("true", ExpectedResult = "\"true\"")]
        [TestCase("false", ExpectedResult = "\"false\"")]
        [TestCase("null", ExpectedResult = "\"null\"")]
        [TestCase(" hoge", ExpectedResult = "\" hoge\"")]
        [TestCase("hoge ", ExpectedResult = "\"hoge \"")]
        [TestCase("&hoge", ExpectedResult = "\"&hoge\"")]
        [TestCase("*hoge", ExpectedResult = "\"*hoge\"")]
        [TestCase("| aaa", ExpectedResult = "\"| aaa\"")]
        [TestCase("- aaa", ExpectedResult = "\"- aaa\"")]
        [TestCase("aaa: bbb", ExpectedResult = "\"aaa: bbb\"")]
        [TestCase("aaa\"bbb", ExpectedResult = "\"aaa\\\"bbb\"")]
        [TestCase("aaa[bbb]", ExpectedResult = "\"aaa[bbb]\"")]
        [TestCase("http://example.com#bbb", ExpectedResult = "\"http://example.com#bbb\"")]
        public string WriteString_AutoDoubleQuoted(string value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteString(value);
            return ToString(in emitter);
        }

        [Test]
        [TestCase("aaa\nbbb\n", ExpectedResult = "|\n  aaa\n  bbb\n")]
        public string WriteString_AutoMultiLines(string value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteString(value);
            return ToString(in emitter);
        }

        [Test]
        public void BlockSequence()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            emitter.WriteInt32(100);
            emitter.WriteInt32(200);
            emitter.WriteInt32(300);
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "- 100\n" +
                "- 200\n" +
                "- 300\n"
                ));
        }

        [Test]
        public void BlockSequence_Empty()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "[]"
            ));
        }

        [Test]
        public void BlockSequence_Nested1()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            {
                emitter.WriteInt32(100);
                emitter.BeginSequence();
                {
                    emitter.WriteInt32(200);
                    emitter.WriteInt32(300);
                }
                emitter.EndSequence();
                emitter.WriteInt32(400);
            }
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "- 100\n" +
                "- \n" +
                "  - 200\n" +
                "  - 300\n" +
                "- 400\n"
            ));
        }

        [Test]
        public void BlockSequence_NestedDeeply()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            {
                emitter.WriteInt32(100);
                emitter.BeginSequence();
                {
                    emitter.WriteInt32(200);
                    emitter.WriteInt32(300);
                }
                emitter.EndSequence();
                emitter.WriteInt32(400);
                emitter.BeginSequence();
                {
                    emitter.WriteInt32(500);
                    emitter.BeginSequence();
                    {
                        emitter.BeginSequence();
                        {
                            emitter.WriteInt32(600);
                            emitter.WriteInt32(700);

                            emitter.BeginSequence();
                            emitter.EndSequence();

                            emitter.BeginSequence(SequenceStyle.Flow);
                            emitter.EndSequence();
                        }
                        emitter.EndSequence();
                    }
                    emitter.EndSequence();
                }
                emitter.EndSequence();
                emitter.WriteInt32(800);
            }
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "- 100\n" +
                "- \n" +
                "  - 200\n" +
                "  - 300\n" +
                "- 400\n" +
                "- \n" +
                "  - 500\n" +
                "  - \n" +
                "    - \n" +
                "      - 600\n" +
                "      - 700\n" +
                "      - []\n" +
                "      - []\n" +
                "- 800\n"
            ));
        }

        [Test]
        public void BlockSequence_NestedFirstElement()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            {
                emitter.BeginSequence();
                {
                    emitter.BeginSequence();
                    {
                        emitter.BeginSequence();
                        {
                            emitter.BeginSequence();
                            {
                                emitter.WriteString("aaa");
                            }
                            emitter.EndSequence();
                        }
                        emitter.EndSequence();
                    }
                    emitter.EndSequence();
                }
                emitter.EndSequence();

                emitter.BeginSequence();
                {
                    emitter.BeginSequence();
                    {
                        emitter.BeginSequence();
                        {
                            emitter.BeginSequence();
                            emitter.EndSequence();
                        }
                        emitter.EndSequence();
                    }
                    emitter.EndSequence();
                }
                emitter.EndSequence();

                emitter.BeginSequence();
                {
                    emitter.BeginSequence();
                    {
                        emitter.BeginSequence();
                        {
                            emitter.BeginMapping();
                            emitter.EndMapping();
                        }
                        emitter.EndSequence();
                    }
                    emitter.EndSequence();
                }
                emitter.EndSequence();

                emitter.WriteString("item1");
            }
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "- \n" +
                "  - \n" +
                "    - \n" +
                "      - \n" +
                "        - aaa\n" +
                "- \n" +
                "  - \n" +
                "    - \n" +
                "      - []\n" +
                "- \n" +
                "  - \n" +
                "    - \n" +
                "      - {}\n" +
                "- item1\n"
            ));
        }

        [Test]
        public void BlockSequence_InBlockMapping()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            {
                emitter.WriteString("aaa");
                emitter.BeginMapping();
                {
                    emitter.WriteString("bbb");
                    emitter.BeginSequence();
                    {
                        emitter.WriteInt32(200);
                        emitter.WriteInt32(300);
                    }
                    emitter.EndSequence();
                }
                emitter.EndMapping();
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "aaa: \n" +
                "  bbb: \n" +
                "  - 200\n" +
                "  - 300\n"
            ));
        }

        [Test]
        public void BlockSequence_InBlockMappingMultiple()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            {
                emitter.WriteString("aaa");
                emitter.BeginMapping();
                {
                    emitter.WriteString("bbb");
                    emitter.BeginSequence();
                    {
                        emitter.WriteInt32(200);
                        emitter.WriteInt32(300);
                    }
                    emitter.EndSequence();

                    emitter.WriteString("ccc");
                    emitter.BeginSequence();
                    {
                        emitter.WriteInt32(400);
                        emitter.WriteInt32(500);
                    }
                    emitter.EndSequence();
                }
                emitter.EndMapping();
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "aaa: \n" +
                "  bbb: \n" +
                "  - 200\n" +
                "  - 300\n" +
                "  ccc: \n" +
                "  - 400\n" +
                "  - 500\n"
            ));
        }

        [Test]
        public void BlockSequence_NestedEmptySequences()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            {
                emitter.BeginSequence();
                emitter.EndSequence();

                emitter.BeginSequence();
                emitter.EndSequence();
            }
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "- []\n" +
                "- []\n"
            ));
        }

        [Test]
        public void BlockSequence_InvalidStartInKey()
        {
            Assert.Throws<YamlEmitterException>(() =>
            {
                using var emitter = CreateEmitter();
                emitter.BeginMapping();
                emitter.BeginSequence();
            });
        }

        [Test]
        public void BlockMapping()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            {
                emitter.WriteInt32(1);
                emitter.WriteInt32(100);
                emitter.WriteInt32(2);
                emitter.WriteInt32(200);
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "1: 100\n" +
                "2: 200\n"
                ));
        }

        [Test]
        public void BlockMapping_Empty()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo("{}"));
        }

        [Test]
        public void BlockMapping_Nested1()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            {
                emitter.WriteInt32(1);
                emitter.WriteInt32(100);
                emitter.WriteInt32(2);
                emitter.BeginMapping();
                {
                    emitter.WriteInt32(3);
                    emitter.WriteInt32(300);
                    emitter.WriteInt32(4);
                    emitter.WriteInt32(400);
                }
                emitter.EndMapping();
                emitter.WriteInt32(5);
                emitter.WriteInt32(500);
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "1: 100\n" +
                "2: \n" +
                "  3: 300\n" +
                "  4: 400\n" +
                "5: 500\n"
            ));
        }

        [Test]
        public void BlockMapping_NestedDeeply()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            {
                emitter.WriteString("key1");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key2");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key3");
                        emitter.BeginMapping();
                        {
                            emitter.WriteString("key4");
                            emitter.BeginSequence();
                            {
                                emitter.WriteInt32(111);
                                emitter.WriteInt32(222);
                            }
                            emitter.EndSequence();
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndMapping();
                }
                emitter.EndMapping();

                emitter.WriteString("key5");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key6");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key7");
                        emitter.BeginMapping();
                        {
                            emitter.WriteString("key8");
                            emitter.BeginSequence();
                            emitter.EndSequence();
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndMapping();
                }
                emitter.EndMapping();

                emitter.WriteString("key9");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key10");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key11");
                        emitter.BeginMapping();
                        {
                            emitter.WriteString("key12");
                            emitter.BeginMapping();
                            emitter.EndMapping();
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndMapping();
                }
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "key1: \n" +
                "  key2: \n" +
                "    key3: \n" +
                "      key4: \n" +
                "      - 111\n" +
                "      - 222\n" +
                "key5: \n" +
                "  key6: \n" +
                "    key7: \n" +
                "      key8: []\n" +
                "key9: \n" +
                "  key10: \n" +
                "    key11: \n" +
                "      key12: {}\n"
            ));
        }

        [Test]
        public void BlockMapping_NestedEmptyMappings()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            {
                emitter.WriteString("a");
                emitter.BeginMapping();
                emitter.EndMapping();

                emitter.WriteString("b");
                emitter.BeginMapping();
                emitter.EndMapping();
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "a: {}\n" +
                "b: {}\n"
            ));
        }

        [Test]
        public void BlockMapping_NestedEmptySequences()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            {
                emitter.WriteString("a");
                emitter.BeginSequence();
                emitter.EndSequence();

                emitter.WriteString("b");
                emitter.BeginSequence();
                emitter.EndSequence();
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "a: []\n" +
                "b: []\n"
            ));
        }

        [Test]
        public void BlockMapping_NestedFirstElements()
        {
            using var emitter = CreateEmitter();
            emitter.BeginMapping();
            {
                emitter.WriteString("key1");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key2");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key3");
                        emitter.BeginMapping();
                        {
                            emitter.WriteString("key4");
                            emitter.WriteString("aaa");
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndMapping();
                }
                emitter.EndMapping();

                emitter.WriteString("key5");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key6");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key7");
                        emitter.BeginMapping();
                        {
                            emitter.WriteString("key8");
                            emitter.BeginSequence();
                            emitter.EndSequence();
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndMapping();
                }
                emitter.EndMapping();

                emitter.WriteString("key9");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key10");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key11");
                        emitter.BeginMapping();
                        {
                            emitter.WriteString("key12");
                            emitter.BeginSequence(SequenceStyle.Flow);
                            emitter.EndSequence();
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndMapping();
                }
                emitter.EndMapping();

                emitter.WriteString("key13");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key14");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key15");
                        emitter.BeginMapping();
                        {
                            emitter.WriteString("key16");
                            emitter.BeginMapping();
                            emitter.EndMapping();
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndMapping();
                }
                emitter.EndMapping();
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "key1: \n" +
                "  key2: \n" +
                "    key3: \n" +
                "      key4: aaa\n" +
                "key5: \n" +
                "  key6: \n" +
                "    key7: \n" +
                "      key8: []\n" +
                "key9: \n" +
                "  key10: \n" +
                "    key11: \n" +
                "      key12: []\n" +
                "key13: \n" +
                "  key14: \n" +
                "    key15: \n" +
                "      key16: {}\n"
            ));
        }

        [Test]
        public void BlockMapping_InvalidStartInKey()
        {
            Assert.Throws<YamlEmitterException>(() =>
            {
                using var emitter = CreateEmitter();
                emitter.BeginMapping();
                emitter.BeginMapping();
            });
        }

        [Test]
        public void BlockMapping_WithEmptyTag()
        {
            using var emitter = CreateEmitter();
            emitter.Tag("!impl1");
            emitter.BeginMapping();
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "!impl1 {}"
            ));
        }

        [Test]
        public void BlockMapping_WithTag()
        {
            using var emitter = CreateEmitter();
            emitter.Tag("!impl1");
            emitter.BeginMapping();
            {
                emitter.WriteString("key1");
                emitter.WriteString("value1");
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "!impl1\n" +
                "key1: value1\n"
                ));
        }

        [Test]
        public void BlockMapping_WithTagInSequence()
        {
            using var emitter = CreateEmitter();

            emitter.BeginSequence();
            {
                emitter.Tag("!impl1");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key1");
                    emitter.WriteString("value1");
                }
                emitter.EndMapping();

                emitter.BeginSequence();
                {
                    emitter.BeginSequence();
                    {
                        emitter.Tag("!impl2");
                        emitter.BeginMapping();
                        {
                            emitter.WriteString("key2");
                            emitter.WriteString("value2");
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndSequence();
                }
                emitter.EndSequence();
            }
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "- !impl1\n" +
                "  key1: value1\n" +
                "- \n" +
                "  - \n" +
                "    - !impl2\n" +
                "    key2: value2\n"
            ));
        }

        [Test]
        public void BlockMapping_WithTagNested()
        {
            using var emitter = CreateEmitter();

            emitter.BeginMapping();
            {
                emitter.WriteString("key1");
                emitter.Tag("!impl1");
                emitter.BeginMapping();
                {
                    emitter.WriteString("key2");
                    emitter.WriteString("value2");

                    emitter.WriteString("key3");
                    emitter.WriteString("value3");

                    emitter.WriteString("key4");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key5");
                        emitter.Tag("!impl2");
                        emitter.BeginMapping();
                        {
                        }
                        emitter.EndMapping();
                    }
                    emitter.EndMapping();
                }
                emitter.EndMapping();
            }
            emitter.EndMapping();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "key1: !impl1\n" +
                "  key2: value2\n" +
                "  key3: value3\n" +
                "  key4: \n" +
                "    key5: !impl2 {}\n"
            ));
        }

        [Test]
        public void FlowSequence()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence(SequenceStyle.Flow);
            {
                emitter.WriteInt32(100);
                emitter.WriteInt32(200);
                emitter.WriteInt32(300);
            }
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "[100, 200, 300]"
                ));
        }

        [Test]
        public void FlowSequence_Nested1()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence(SequenceStyle.Flow);
            {
                emitter.WriteInt32(100);
                emitter.BeginSequence(SequenceStyle.Flow);
                {
                    emitter.WriteInt32(200);
                }
                emitter.EndSequence();
                emitter.WriteInt32(300);
            }

            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "[100, [200], 300]"
            ));
        }

        [Test]
        public void ComplexStructure()
        {
            using var emitter = CreateEmitter();
            emitter.BeginSequence();
            {
                emitter.BeginSequence(SequenceStyle.Flow);
                {
                    emitter.WriteInt32(100);
                    emitter.WriteString("&hoge");
                    emitter.WriteString("bra");
                }
                emitter.EndSequence();

                emitter.BeginMapping();
                {
                    emitter.WriteString("key1");
                    emitter.WriteString("item1");

                    emitter.WriteString("key2");
                    emitter.BeginSequence(SequenceStyle.Flow);
                    emitter.EndSequence();

                    emitter.WriteString("key3");
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("key4");
                        emitter.WriteInt32(400);

                        emitter.WriteString("key5");
                        emitter.BeginSequence();
                        emitter.EndSequence();

                        emitter.WriteString("key6");
                        emitter.BeginSequence();
                        {
                            emitter.WriteInt32(600);
                            emitter.BeginMapping();
                            {
                                emitter.WriteString("aaa");
                                emitter.WriteString("AAAAAAAAAAA\nHEYHEYHEYHEYHEY\n");

                                emitter.WriteString("ccc");
                                emitter.BeginSequence();
                                {
                                    emitter.BeginSequence();
                                    emitter.EndSequence();

                                    emitter.BeginSequence(SequenceStyle.Flow);
                                    emitter.EndSequence();

                                    emitter.BeginMapping();
                                    emitter.EndMapping();

                                    emitter.WriteFloat(1.234f);
                                    emitter.WriteString("Hello\nWorWorWorWorld\n");
                                }
                                emitter.EndSequence();
                            }
                            emitter.EndMapping();
                        }
                        emitter.EndSequence();
                    }
                    emitter.EndMapping();
                }
                emitter.EndMapping();
            }
            emitter.EndSequence();

            Assert.That(ToString(in emitter), Is.EqualTo(
                "- [100, \"&hoge\", bra]\n" +
                "- key1: item1\n" +
                "  key2: []\n" +
                "  key3: \n" +
                "    key4: 400\n" +
                "    key5: []\n" +
                "    key6: \n" +
                "    - 600\n" +
                "    - aaa: |\n" +
                "        AAAAAAAAAAA\n" +
                "        HEYHEYHEYHEYHEY\n" +
                "      ccc: \n" +
                "      - []\n" +
                "      - []\n" +
                "      - {}\n" +
                "      - 1.234\n" +
                "      - |\n" +
                "        Hello\n" +
                "        WorWorWorWorld\n"
            ));
        }

        static Utf8YamlEmitter CreateEmitter(YamlEmitOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>(256);
            return new Utf8YamlEmitter(bufferWriter, options);
        }

        static string ToString(in Utf8YamlEmitter emitter)
        {
            var writer = (ArrayBufferWriter<byte>)emitter.GetWriter();
            return StringEncoding.Utf8.GetString(writer.WrittenSpan);
        }
    }
}
