using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using VYaml.Internal;

namespace VYaml.Tests
{
    [TestFixture]
    public class StreamHelperTest
    {
        [Test]
        public async Task ReadAsSequenceAsync_MemoryStream()
        {
            var memoryStream = new MemoryStream(new [] { (byte)'a', (byte)'b', (byte)'c' });
            var builder = await StreamHelper.ReadAsSequenceAsync(memoryStream);
            try
            {
                var sequence = builder.Build();
                Assert.That(sequence.IsSingleSegment, Is.True);
                Assert.That(sequence.Length, Is.EqualTo(3));
                Assert.That(sequence.First.Span[0], Is.EqualTo((byte)'a'));
                Assert.That(sequence.First.Span[1], Is.EqualTo((byte)'b'));
                Assert.That(sequence.First.Span[2], Is.EqualTo((byte)'c'));
            }
            finally
            {
                ReusableByteSequenceBuilderPool.Return(builder);
            }
        }

        [Test]
        public async Task ReadAsSequenceAsync_FileStream()
        {
            var tempFilePath = Path.GetTempFileName();
#if NETFRAMEWORK
            File.WriteAllText(tempFilePath, new string('a', 1000));
            using var fileStream = File.OpenRead(tempFilePath);
#else
            await File.WriteAllTextAsync(tempFilePath, new string('a', 1000));
            await using var fileStream = File.OpenRead(tempFilePath);
#endif
            var builder = await StreamHelper.ReadAsSequenceAsync(fileStream);
            try
            {
                var sequence = builder.Build();
                Assert.That(sequence.Length, Is.EqualTo(1000));
                foreach (var readOnlyMemory in sequence)
                {
                    foreach (var b in readOnlyMemory.Span.ToArray())
                    {
                        Assert.That(b, Is.EqualTo('a'));
                    }
                }
            }
            finally
            {
                ReusableByteSequenceBuilderPool.Return(builder);
            }
        }
    }
}