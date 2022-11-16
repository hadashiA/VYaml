using System;
using NUnit.Framework;
using VYaml.Internal;

namespace VYaml.Tests;

[TestFixture]
class InsertionQueueTest
{
    [Test]
    public void Enqueue()
    {
        var q = new InsertionQueue<int>(4);
        q.Enqueue(100);
        q.Enqueue(200);
        q.Enqueue(300);
        q.Enqueue(400);
            
        Assert.That(q.Dequeue(), Is.EqualTo(100));
        Assert.That(q.Dequeue(), Is.EqualTo(200));
        Assert.That(q.Dequeue(), Is.EqualTo(300));
        Assert.That(q.Dequeue(), Is.EqualTo(400));
        Assert.Throws<InvalidOperationException>(() => q.Dequeue());
    }

    [Test]
    public void Insert()
    {
        var q = new InsertionQueue<int>(4);
        q.Enqueue(100);
        q.Enqueue(200);
        q.Enqueue(300);
        q.Enqueue(400);
            
        q.Insert(1, 999);
            
        Assert.That(q.Dequeue(), Is.EqualTo(100));
        Assert.That(q.Dequeue(), Is.EqualTo(999));
        Assert.That(q.Dequeue(), Is.EqualTo(200));
        Assert.That(q.Dequeue(), Is.EqualTo(300));
        Assert.That(q.Dequeue(), Is.EqualTo(400));
    }

    [Test]
    public void Insert_ProgressingBuffer()
    {
        var q = new InsertionQueue<int>(4);
        q.Enqueue(100);
        q.Enqueue(200);
        q.Dequeue();
        q.Dequeue();
            
        q.Enqueue(100);
        q.Enqueue(200);

        q.Insert(0, 999);
        Assert.That(q.Dequeue(), Is.EqualTo(999));
    }
}