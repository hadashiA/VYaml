using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VYaml.Parser;

namespace VYaml.Tests.Parser
{
    [TestFixture]
    public class YamlParserTest
    {
        [Test]
        public void IsNullScalar()
        {
            var parser = CreateParser(new []
            {
                "- null",
                "- ",
                "- ~",
                "- not null",
            });

            parser.SkipAfter(ParseEventType.DocumentStart);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceStart));
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.True);
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.True);
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.True);
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.False);
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceEnd));
        }

        [Test]
        public void SkipCurrentNode()
        {
            var parser = CreateParser(new []
            {
                "a: 1",
                "b: { ba: 2 }",
                "c: { ca: [100, 200, 300] }",
                "d: { da: [100, 200, 300], db: 100 }",
                "e: { ea: [{eaa: 100}, 200, 300], db: {} }",
                "f: [{ fa: 100, fb: [100, 200, 300] }]",
            });

            parser.SkipAfter(ParseEventType.MappingStart);
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("a"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("b"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("c"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("d"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("e"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("f"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
        }

        [Test]
        public void Tag_BlockMapping()
        {
            var parser = CreateParser(new []
            {
                "!tag1",
                "a: 100",
                "b: 200",
            });

            parser.SkipAfter(ParseEventType.DocumentStart);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            Assert.That(parser.TryGetCurrentTag(out var tag), Is.True);
            Assert.That(tag.ToString(), Is.EqualTo("!tag1"));
        }

        [Test]
        public void UnityFormat()
        {
            var parser = CreateParser(new []
            {
                "%YAML 1.1",
                "%TAG !u! tag:unity3d.com,2011:",
                "--- !u!29 &1",
                "OcclusionCullingSettings:",
                "  m_ObjectHideFlags: 0",
                "  serializedVersion: 2",
                "  m_OcclusionBakeSettings:",
                "    smallestOccluder: 5",
                "    smallestHole: 0.25",
                "    backfaceThreshold: 100",
                "  m_SceneGUID: 00000000000000000000000000000000",
                "  m_OcclusionCullingData: {fileID: 0}",
                "--- !u!104 &2",
                "RenderSettings:",
                "  m_ObjectHideFlags: 0",
                "  serializedVersion: 9",
                "--- !u!4 &62555683 stripped",
                "Transform:",
                "  m_CorrespondingSourceObject: {fileID: 180319434217191821, guid: 0f48f06ff1ceb490892217c1fb56ad67,",
                "    type: 3}",
                "  m_PrefabInstance: {fileID: 62555682}",
                "  m_PrefabAsset: {fileID: 0}",
                "--- !u!4 &65300469 stripped",
                "Transform:",
                "  m_CorrespondingSourceObject: {fileID: 180319434217191821, guid: 89d43c10426ce4d61b44a4261ce6e27d,",
                "    type: 3}",
                "  m_PrefabInstance: {fileID: 65300468}",
                "  m_PrefabAsset: {fileID: 0}",
                "--- !u!1001 &91330633",
                "PrefabInstance:",
                "  m_ObjectHideFlags: 0",
                "  serializedVersion: 2",
                "  m_Modification:",
                "    serializedVersion: 2",
            });

            parser.SkipAfter(ParseEventType.StreamStart);

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("OcclusionCullingSettings"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("RenderSettings"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("Transform"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("Transform"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("PrefabInstance"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.StreamEnd));
            Assert.That(parser.Read(), Is.False);
        }

        [Test]
        public void EmptyElementInSequence()
        {
            var parser = CreateParser(new []
            {
                "keywords:",
                "- ",
                "- _RIDE_ON",
                "- _COME_ON",
            });

            parser.SkipAfter(ParseEventType.DocumentStart);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("keywords"));
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceStart));
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.True);
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("_RIDE_ON"));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("_COME_ON"));
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
        }

        [Test]
        public void QuotedNumberMustNotBeAnInteger()
        {
            YamlParser parser = CreateParser(new [] {
                "- '012345'",
                "- 012345",
            });

            parser.SkipAfter(ParseEventType.SequenceStart);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.TryReadScalarAsInt32(out _), Is.EqualTo(false));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("012345"));

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.TryReadScalarAsInt32(out int value), Is.EqualTo(true));
            Assert.That(value, Is.EqualTo(012345));
        }

        static YamlParser CreateParser(IEnumerable<string> lines)
        {
            var yaml = string.Join("\n", lines);
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml));
            return new YamlParser(sequence);
        }

        static void CreateParser(string yaml, out YamlParser x)
        {
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml));
            x = new YamlParser(sequence);
        }
    }
}