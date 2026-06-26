using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VYaml.Tests.TestSuite
{
    /// <summary>
    /// A single leaf test of the yaml-test-suite (a directory containing <c>in.yaml</c>).
    /// </summary>
    public sealed class YamlTestSuiteCase
    {
        public string Id { get; }
        public string Label { get; }
        public byte[] Yaml { get; }
        public bool IsError { get; }
        public IReadOnlyList<YamlTestSuiteEvent> ExpectedEvents { get; }

        YamlTestSuiteCase(string id, string label, byte[] yaml, bool isError, IReadOnlyList<YamlTestSuiteEvent> expectedEvents)
        {
            Id = id;
            Label = label;
            Yaml = yaml;
            IsError = isError;
            ExpectedEvents = expectedEvents;
        }

        public override string ToString() => $"{Id} ({Label})";

        public static IEnumerable<YamlTestSuiteCase> LoadAll(string suiteRoot)
        {
            foreach (var dir in EnumerateLeafDirectories(suiteRoot).OrderBy(x => x))
            {
                var inYaml = Path.Combine(dir, "in.yaml");
                var eventFile = Path.Combine(dir, "test.event");
                if (!File.Exists(inYaml) || !File.Exists(eventFile))
                {
                    continue;
                }

                var id = Path.GetRelativePath(suiteRoot, dir).Replace('\\', '/');
                var labelFile = Path.Combine(dir, "===");
                var label = File.Exists(labelFile) ? File.ReadAllText(labelFile).Trim() : id;
                var isError = File.Exists(Path.Combine(dir, "error"));
                var events = YamlTestSuiteEvent.ParseAll(File.ReadAllText(eventFile));

                yield return new YamlTestSuiteCase(id, label, File.ReadAllBytes(inYaml), isError, events);
            }
        }

        static IEnumerable<string> EnumerateLeafDirectories(string root)
        {
            // Leaf tests are directories that directly contain `in.yaml`.
            // Most live at the top level (e.g. `229Q`), some are nested one level (e.g. `3RLN/01`).
            foreach (var dir in Directory.EnumerateDirectories(root))
            {
                var name = Path.GetFileName(dir);
                if (name.StartsWith(".")) // skip .git etc.
                {
                    continue;
                }
                // `name/` and `tags/` re-group the canonical hash-id tests via symlinks; skip the duplicates.
                if (name == "name" || name == "tags")
                {
                    continue;
                }
                if (File.Exists(Path.Combine(dir, "in.yaml")))
                {
                    yield return dir;
                }
                foreach (var sub in Directory.EnumerateDirectories(dir))
                {
                    if (File.Exists(Path.Combine(sub, "in.yaml")))
                    {
                        yield return sub;
                    }
                }
            }
        }
    }
}
