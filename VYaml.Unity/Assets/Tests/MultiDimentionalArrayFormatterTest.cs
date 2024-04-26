using NUnit.Framework;
using VYaml.Tests.Serialization;

namespace VYaml.Tests
{
    [TestFixture]
    public class MultiDimentionalArrayFormatterTest : FormatterTestBase
    {
        [Test]
        public void TwoDimention()
        {
            var value = new int[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
            var serialized = Serialize(value);
            Assert.That(serialized, Is.EqualTo(
                "- \n" +
                "  - 1\n" +
                "  - 2\n" +
                "  - 3\n" +
                "- \n" +
                "  - 4\n" +
                "  - 5\n" +
                "  - 6\n"));

            var deserialized = Deserialize<int[,]>(serialized);
            Assert.That(deserialized.GetLength(0), Is.EqualTo(2));
            Assert.That(deserialized.GetLength(1), Is.EqualTo(3));
            Assert.That(deserialized[0, 0], Is.EqualTo(1));
            Assert.That(deserialized[0, 1], Is.EqualTo(2));
            Assert.That(deserialized[0, 2], Is.EqualTo(3));
            Assert.That(deserialized[1, 0], Is.EqualTo(4));
            Assert.That(deserialized[1, 1], Is.EqualTo(5));
            Assert.That(deserialized[1, 2], Is.EqualTo(6));
        }

        [Test]
        public void ThreeDimention()
        {
            var value = new int[4, 3, 2]
            {
                { { 1, 2 }, { 3, 4 }, { 5, 6 } },
                { { 7, 8 }, { 9, 0 }, { 1, 2 } },
                { { 3, 4 }, { 5, 6 }, { 7, 8 } },
                { { 9, 0 }, { 1, 2 }, { 3, 4 } },
            };
            var serialized = Serialize(value);
            Assert.That(serialized, Is.EqualTo(
                "- \n" +
                "  - \n" +
                "    - 1\n" +
                "    - 2\n" +
                "  - \n" +
                "    - 3\n" +
                "    - 4\n" +
                "  - \n" +
                "    - 5\n" +
                "    - 6\n" +
                "- \n" +
                "  - \n" +
                "    - 7\n" +
                "    - 8\n" +
                "  - \n" +
                "    - 9\n" +
                "    - 0\n" +
                "  - \n" +
                "    - 1\n" +
                "    - 2\n" +
                "- \n" +
                "  - \n" +
                "    - 3\n" +
                "    - 4\n" +
                "  - \n" +
                "    - 5\n" +
                "    - 6\n" +
                "  - \n" +
                "    - 7\n" +
                "    - 8\n" +
                "- \n" +
                "  - \n" +
                "    - 9\n" +
                "    - 0\n" +
                "  - \n" +
                "    - 1\n" +
                "    - 2\n" +
                "  - \n" +
                "    - 3\n" +
                "    - 4\n"
                ));

            var deserialized = Deserialize<int[,,]>(serialized);
            Assert.That(deserialized.GetLength(0), Is.EqualTo(4));
            Assert.That(deserialized.GetLength(1), Is.EqualTo(3));
            Assert.That(deserialized.GetLength(2), Is.EqualTo(2));
            Assert.That(deserialized[0, 0, 0], Is.EqualTo(1));
            Assert.That(deserialized[0, 1, 1], Is.EqualTo(4));
            Assert.That(deserialized[2, 1, 1], Is.EqualTo(6));
        }
    }
}