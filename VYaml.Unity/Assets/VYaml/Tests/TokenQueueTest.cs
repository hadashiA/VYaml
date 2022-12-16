using System;
using NUnit.Framework;
using VYaml.Internal;
using VYaml.Parser;

namespace VYaml.Tests
{
    [TestFixture]
    class TokenQueueTest
    {
        [Test]
        public void Enqueue_TypeOnly()
        {
            var q = new TokenQueue(2);

            q.Enqueue(TokenType.FlowSequenceStart);
            q.Enqueue(TokenType.FlowSequenceEnd);
            q.Enqueue(TokenType.FlowSequenceStart);
            q.Enqueue(TokenType.FlowSequenceEnd);

            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.FlowSequenceStart, default(ITokenContent?))));
            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.FlowSequenceEnd, default(ITokenContent?))));
            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.FlowSequenceStart, default(ITokenContent?))));
            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.FlowSequenceEnd, default(ITokenContent?))));

            Exception? ex = null;
            try { q.Dequeue(); }
            catch (Exception dequeueEx) { ex = dequeueEx; }

            Assert.That(ex, Is.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void Enqueue_WithContent()
        {
            var q = new TokenQueue(2);

            q.Enqueue(TokenType.FlowSequenceStart);
            q.Enqueue(TokenType.PlainScalar, new Scalar(new [] { (byte)'a', (byte)'a' }));
            q.Enqueue(TokenType.FoldedScalar, new Scalar(new [] { (byte)'b', (byte)'b' }));
            q.Enqueue(TokenType.FlowSequenceEnd);

            var (type1, content1) = q.Dequeue();
            Assert.That(type1, Is.EqualTo(TokenType.FlowSequenceStart));
            Assert.That(content1, Is.Null);

            var (type2, content2) = q.Dequeue();
            Assert.That(type2, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(content2, Is.InstanceOf<Scalar>());
            Assert.That(content2!.ToString(), Is.EqualTo("aa"));

            var (type3, content3) = q.Dequeue();
            Assert.That(type3, Is.EqualTo(TokenType.FoldedScalar));
            Assert.That(content3, Is.InstanceOf<Scalar>());
            Assert.That(content3!.ToString(), Is.EqualTo("bb"));

            var (type4, content4) = q.Dequeue();
            Assert.That(type4, Is.EqualTo(TokenType.FlowSequenceEnd));
            Assert.That(content4, Is.Null);

            Exception ex = null;
            try { q.Dequeue(); }
            catch (Exception dequeueEx) { ex = dequeueEx; }

            Assert.That(ex, Is.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void Insert_TypeOnly()
        {
            var q = new TokenQueue(2);
            q.Enqueue(TokenType.FoldedScalar);
            q.Enqueue(TokenType.LiteralScalar);
            q.Enqueue(TokenType.PlainScalar);

            q.Insert(1, TokenType.KeyStart);

            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.FoldedScalar, default(ITokenContent?))));
            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.KeyStart, default(ITokenContent?))));
            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.LiteralScalar, default(ITokenContent?))));
            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.PlainScalar, default(ITokenContent?))));
        }

        [Test]
        public void Insert_WithContent()
        {
            var q = new TokenQueue(2);
            q.Enqueue(TokenType.FoldedScalar);
            q.Enqueue(TokenType.LiteralScalar);
            q.Enqueue(TokenType.PlainScalar);

            q.Insert(1, TokenType.PlainScalar, new Scalar(new [] { (byte)'a', (byte)'b' }));

            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.FoldedScalar, default(ITokenContent?))));

            var (type, content) = q.Dequeue();
            Assert.That(type, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(content, Is.InstanceOf<Scalar>());
            Assert.That(content!.ToString(), Is.EqualTo("ab"));

            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.LiteralScalar, default(ITokenContent?))));
            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.PlainScalar, default(ITokenContent?))));
        }

        [Test]
        public void Insert_ProgressingBuffer()
        {
            var q = new TokenQueue(2);
            q.Enqueue(TokenType.FlowSequenceStart);
            q.Enqueue(TokenType.ValueStart);
            q.Dequeue();
            q.Dequeue();

            q.Enqueue(TokenType.PlainScalar, new Scalar(new [] { (byte)'a', (byte)'a' }));
            q.Enqueue(TokenType.FoldedScalar, new Scalar(new [] { (byte)'b', (byte)'b' }));

            q.Insert(0, TokenType.KeyStart);
            Assert.That(q.Dequeue(), Is.EqualTo((TokenType.KeyStart, default(ITokenContent?))));
        }
    }
}
