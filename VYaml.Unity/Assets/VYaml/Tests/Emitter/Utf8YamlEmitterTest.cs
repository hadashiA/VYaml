using System;
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
        [TestCase(true, ExpectedResult = "true")]
        [TestCase(false, ExpectedResult = "false")]
        public string WriteBool(bool value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteBool(value);
            return StringResult(in emitter);
        }

        [Test]
        [TestCase(123, ExpectedResult = "123")]
        [TestCase(-123, ExpectedResult = "-123")]
        public string WriteInt32(int value)
        {
            using var emitter = CreateEmitter();
            emitter.WriteInt32(value);
            return StringResult(in emitter);
        }

        [Test]
        public void BlockSequence()
        {
            using var emitter = CreateEmitter();
            emitter.BeginBlockSequence();
            emitter.WriteInt32(100);
            emitter.WriteInt32(200);
            emitter.WriteInt32(300);
            emitter.EndBlockSequence();

            Assert.That(StringResult(in emitter), Is.EqualTo(
                "- 100\n" +
                "- 200\n" +
                "- 300\n"
                ));
        }

        [Test]
        public void BlockSequence_Nested1()
        {
            using var emitter = CreateEmitter();
            emitter.BeginBlockSequence();
            {
                emitter.WriteInt32(100);
                emitter.BeginBlockSequence();
                {
                    emitter.WriteInt32(200);
                    emitter.WriteInt32(300);
                }
                emitter.EndBlockSequence();
                emitter.WriteInt32(400);
            }
            emitter.EndBlockSequence();

            Assert.That(StringResult(in emitter), Is.EqualTo(
                "- 100\n" +
                "-\n" +
                "  - 200\n" +
                "  - 300\n" +
                "- 400\n"
            ));
        }

        [Test]
        public void BlockSequence_Nested2()
        {
            using var emitter = CreateEmitter();
            emitter.BeginBlockSequence();
            {
                emitter.WriteInt32(100);
                emitter.BeginBlockSequence();
                {
                    emitter.WriteInt32(200);
                    emitter.WriteInt32(300);
                }
                emitter.EndBlockSequence();
                emitter.WriteInt32(400);
                emitter.BeginBlockSequence();
                {
                    emitter.WriteInt32(500);
                    emitter.BeginBlockSequence();
                    {
                        emitter.WriteInt32(600);
                    }
                    emitter.EndBlockSequence();
                }
                emitter.EndBlockSequence();
                emitter.WriteInt32(700);
            }
            emitter.EndBlockSequence();

            Assert.That(StringResult(in emitter), Is.EqualTo(
                "- 100\n" +
                "-\n" +
                "  - 200\n" +
                "  - 300\n" +
                "- 400\n" +
                "-\n" +
                "  - 500\n" +
                "  -\n" +
                "    - 600\n" +
                "- 700\n"
            ));
        }

        [Test]
        public void BlockMapping()
        {
            var emitter = CreateEmitter();
            emitter.BeginBlockMapping();
            emitter.WriteInt32(1);
            emitter.WriteInt32(100);
            emitter.WriteInt32(2);
            emitter.WriteInt32(200);
            emitter.EndBlockMapping();

            Assert.That(StringResult(in emitter), Is.EqualTo(
                "1: 100\n" +
                "2: 200\n"
                ));
        }

        static Utf8YamlEmitter CreateEmitter()
        {
            var bufferWriter = new ArrayBufferWriter<byte>(256);
            return new Utf8YamlEmitter(bufferWriter);
        }

        static string StringResult(in Utf8YamlEmitter emitter)
        {
            var writer = (ArrayBufferWriter<byte>)emitter.GetWriter();
            return StringEncoding.Utf8.GetString(writer.WrittenSpan);
        }
    }
}
