using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using VYaml.Parser;

namespace VYaml.Tests.TestSuite
{
    /// <summary>
    /// Runs the official yaml-test-suite (<c>data</c> branch, checked out as a git submodule)
    /// against <see cref="YamlParser"/>, comparing emitted parse events with the expected
    /// <c>test.event</c> stream.
    /// </summary>
    /// <see href="https://github.com/yaml/yaml-test-suite" />
    [TestFixture]
    public class YamlTestSuiteTest
    {
        public static IEnumerable<TestCaseData> EventCases()
        {
            var root = TryFindSuiteRoot();
            if (root is null)
            {
                yield return new TestCaseData((YamlTestSuiteCase?)null)
                    .SetName("yaml-test-suite submodule not checked out")
                    .Ignore("yaml-test-suite submodule is not checked out. Run: git submodule update --init");
                yield break;
            }

            foreach (var testCase in YamlTestSuiteCase.LoadAll(root))
            {
                var data = new TestCaseData(testCase).SetName($"{testCase.Id}: {testCase.Label}");
                if (YamlTestSuiteSkipList.Skipped.TryGetValue(testCase.Id, out var reason))
                {
                    data.Ignore(reason);
                }
                yield return data;
            }
        }

        [TestCaseSource(nameof(EventCases))]
        public void Conformance(YamlTestSuiteCase? testCase)
        {
            if (testCase is null)
            {
                return; // placeholder case (submodule missing) is ignored above.
            }

            if (testCase.IsError)
            {
                AssertParseError(testCase);
            }
            else
            {
                AssertParseEvents(testCase);
            }
        }

        static void AssertParseEvents(YamlTestSuiteCase testCase)
        {
            var parser = YamlParser.FromBytes(testCase.Yaml);
            var index = 0;
            foreach (var expected in testCase.ExpectedEvents)
            {
                if (!parser.Read())
                {
                    Assert.Fail($"[{index}] expected {expected} but the stream ended");
                }

                var actualType = MapEventType(parser.CurrentEventType);
                if (actualType != expected.Type)
                {
                    Assert.Fail($"[{index}] expected event {expected.Type} ({expected}) but was {actualType}");
                }

                if (expected.Type == "=VAL")
                {
                    var actualValue = parser.GetScalarAsString() ?? string.Empty;
                    if (actualValue != expected.Value)
                    {
                        Assert.Fail($"[{index}] scalar value mismatch\n" +
                                    $"  expected: {Quote(expected.Value)}\n" +
                                    $"  but was:  {Quote(actualValue)}");
                    }
                }

                if (expected.Anchor != null)
                {
                    parser.TryGetCurrentAnchor(out var anchor);
                    var actualAnchor = anchor?.Name;
                    if (actualAnchor != expected.Anchor)
                    {
                        Assert.Fail($"[{index}] anchor mismatch for {expected.Type}\n" +
                                    $"  expected: &{expected.Anchor}\n" +
                                    $"  but was:  {(actualAnchor is null ? "<none>" : "&" + actualAnchor)}");
                    }
                }

                index++;
            }

            if (parser.Read())
            {
                Assert.Fail($"[{index}] unexpected extra event {MapEventType(parser.CurrentEventType)} " +
                            $"`{parser.GetScalarAsString()}`");
            }
        }

        static void AssertParseError(YamlTestSuiteCase testCase)
        {
            Assert.That(() =>
            {
                var parser = YamlParser.FromBytes(testCase.Yaml);
                while (parser.Read())
                {
                }
            }, Throws.InstanceOf<Exception>(), $"Expected a parse error for {testCase.Id}, but parsing succeeded");
        }

        static string MapEventType(ParseEventType type) => type switch
        {
            ParseEventType.StreamStart => "+STR",
            ParseEventType.StreamEnd => "-STR",
            ParseEventType.DocumentStart => "+DOC",
            ParseEventType.DocumentEnd => "-DOC",
            ParseEventType.MappingStart => "+MAP",
            ParseEventType.MappingEnd => "-MAP",
            ParseEventType.SequenceStart => "+SEQ",
            ParseEventType.SequenceEnd => "-SEQ",
            ParseEventType.Scalar => "=VAL",
            ParseEventType.Alias => "=ALI",
            _ => type.ToString(),
        };

        static string Quote(string? s) => "\"" + (s ?? string.Empty).Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\t", "\\t") + "\"";

        static string? TryFindSuiteRoot()
        {
            // Walk up from the test assembly location looking for the submodule directory.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "yaml-test-suite");
                if (Directory.Exists(candidate) && Directory.EnumerateDirectories(candidate).Any())
                {
                    // sanity check: a known test exists
                    if (Directory.Exists(Path.Combine(candidate, "229Q")))
                    {
                        return candidate;
                    }
                }
                dir = dir.Parent;
            }
            return null;
        }
    }
}
