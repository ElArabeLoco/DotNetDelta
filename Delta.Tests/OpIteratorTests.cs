namespace DotNetDelta.Tests;

[TestFixture]
public class OpIteratorTests
{
    Delta delta = new Delta();

    [SetUp]
    public void SetUp()
    {
        delta = new Delta()
            .Insert("Hello", new AttributeMap() { { "bold", true } })
            .Retain(3)
            .Insert(new Dictionary<string, object>() { { "embed", (object) 2 } }, new AttributeMap() { { "src", "http://quilljs.com" } })
            .Delete(4);
    }

    [Test]
    public void HasNext_WithNonEmptyOpsFromDelta_ShouldReturnTrue()
    {
        OpIterator iter = new OpIterator(delta.Ops);
        Assert.IsTrue(iter.HasNext());
    }

    [Test]
    public void HasNext_WithEmptyOps_ShouldReturnFalse()
    {
        Assert.IsFalse(new OpIterator(new List<Op>()).HasNext());
    }

    [Test]
    public void PeekLength_WhenIteratingWithoutOffset_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(delta.Ops);
        Assert.AreEqual(5, iter.PeekLength());
        iter.Next();
        Assert.AreEqual(3, iter.PeekLength());
        iter.Next();
        Assert.AreEqual(1, iter.PeekLength());
        iter.Next();
        Assert.AreEqual(4, iter.PeekLength());
    }

    [Test]
    public void PeekLength_WhenIteratingWithOffset_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(delta.Ops);
        iter.Next(2);
        Assert.AreEqual(5 - 2, iter.PeekLength());
    }

    [Test]
    public void PeekLength_WhenNoOpsLeft_ShouldReturnCorrectValue()
    {
        Assert.AreEqual(int.MaxValue, new OpIterator(new List<Op>()).PeekLength());
    }

    [Test]
    public void PeekType_WhenIteratingWithoutOffset_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(delta.Ops);
        Assert.AreEqual(OpType.Insert, iter.PeekType());
        iter.Next();
        Assert.AreEqual(OpType.Retain, iter.PeekType());
        iter.Next();
        Assert.AreEqual(OpType.Insert, iter.PeekType());
        iter.Next();
        Assert.AreEqual(OpType.Delete, iter.PeekType());
    }

    [Test]
    public void Next_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(delta.Ops);
        for (int i = 0; i < delta.Ops.Count; i++)
        {
            Assert.AreEqual(delta.Ops[i], iter.Next());
        }

        Assert.AreEqual(Op.Retain(int.MaxValue), iter.Next());
        Assert.AreEqual(Op.Retain(int.MaxValue), iter.Next(4));
        Assert.AreEqual(Op.Retain(int.MaxValue), iter.Next());
    }

    [Test]
    public void Next_WithOffset_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(delta.Ops);
        Assert.AreEqual(Op.Insert("He", new AttributeMap() { { "bold", true } }), iter.Next(2));
        Assert.AreEqual(Op.Insert("llo", new AttributeMap() { { "bold", true } }), iter.Next(10));

        Assert.AreEqual(Op.Retain(1), iter.Next(1));
        Assert.AreEqual(Op.Retain(2), iter.Next(2));

    }

    [Test]
    public void Rest_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(delta.Ops);
        iter.Next(2);

        List<Op> expected = new List<Op> {
            Op.Insert("llo", new AttributeMap() { { "bold", true } }),
            Op.Retain(3),
            Op.InsertEmbed(new Dictionary<string, object>() { { "embed", (object) 2 } }, new AttributeMap() { { "src", "http://quilljs.com" } }),
            Op.Delete(4)
        };
        Assert.AreEqual(expected, iter.Rest());

        iter.Next(3);
        expected = new List<Op> {
            Op.Retain(3),
            Op.InsertEmbed(new Dictionary<string, object>() { { "embed", (object) 2 } }, new AttributeMap() { { "src", "http://quilljs.com" } }),
            Op.Delete(4)
        };
        Assert.AreEqual(expected, iter.Rest());

        iter.Next(3);
        iter.Next(2);
        iter.Next(4);
        Assert.AreEqual(new List<Op>(), iter.Rest());
    }


}