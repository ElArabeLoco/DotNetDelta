namespace DotNetDelta.Tests;

[TestFixture]
public class OpIteratorTests
{
    Delta _delta = new();

    [SetUp]
    public void SetUp()
    {
        _delta = new Delta()
            .Insert("Hello", new AttributeMap() { { "bold", true } })
            .Retain(3)
            .Insert(new Dictionary<string, object>() { { "embed", (object) 2 } }, new AttributeMap() { { "src", "http://quilljs.com" } })
            .Delete(4);
    }

    [Test]
    public void HasNext_WithNonEmptyOpsFromDelta_ShouldReturnTrue()
    {
        OpIterator iter = new OpIterator(_delta.Ops);
        Assert.That(iter.HasNext(), Is.True);
    }

    [Test]
    public void HasNext_WithEmptyOps_ShouldReturnFalse()
    {
        Assert.IsFalse(new OpIterator(new List<Op>()).HasNext());
    }

    [Test]
    public void PeekLength_WhenIteratingWithoutOffset_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(_delta.Ops);
        Assert.That(iter.PeekLength(), Is.EqualTo(5));
        iter.Next();
        Assert.That(iter.PeekLength(), Is.EqualTo(3));
        iter.Next();
        Assert.That(iter.PeekLength(), Is.EqualTo(1));
        iter.Next();
        Assert.That(iter.PeekLength(), Is.EqualTo(4));
    }

    [Test]
    public void PeekLength_WhenIteratingWithOffset_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(_delta.Ops);
        iter.Next(2);
        Assert.That(iter.PeekLength(), Is.EqualTo(5 - 2));
    }

    [Test]
    public void PeekLength_WhenNoOpsLeft_ShouldReturnCorrectValue()
    {
        Assert.That(new OpIterator(new List<Op>()).PeekLength(), Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void PeekType_WhenIteratingWithoutOffset_ShouldReturnCorrectValue()
    {
        OpIterator iter = new OpIterator(_delta.Ops);
        Assert.That(iter.PeekType(), Is.EqualTo(OpType.Insert));
        iter.Next();
        Assert.That(iter.PeekType(), Is.EqualTo(OpType.Retain));
        iter.Next();
        Assert.That(iter.PeekType(), Is.EqualTo(OpType.Insert));
        iter.Next();
        Assert.That(iter.PeekType(), Is.EqualTo(OpType.Delete));
    }

    [Test]
    public void Next_ShouldReturnCorrectValue()
    {
        var iter = new OpIterator(_delta.Ops);
        foreach (var t in _delta.Ops)
        {
            Assert.That(iter.Next(), Is.EqualTo(t));
        }

        Assert.That(iter.Next(), Is.EqualTo(Op.Retain(int.MaxValue)));
        Assert.That(iter.Next(4), Is.EqualTo(Op.Retain(int.MaxValue)));
        Assert.That(iter.Next(), Is.EqualTo(Op.Retain(int.MaxValue)));
    }

    [Test]
    public void Next_WithOffset_ShouldReturnCorrectValue()
    {
        var iter = new OpIterator(_delta.Ops);
        Assert.That(iter.Next(2), Is.EqualTo(Op.Insert("He", new AttributeMap() { { "bold", true } })));
        Assert.That(iter.Next(10), Is.EqualTo(Op.Insert("llo", new AttributeMap() { { "bold", true } })));

        Assert.That(iter.Next(1), Is.EqualTo(Op.Retain(1)));
        Assert.That(iter.Next(2), Is.EqualTo(Op.Retain(2)));

    }

    [Test]
    public void Rest_ShouldReturnCorrectValue()
    {
        var iter = new OpIterator(_delta.Ops);
        iter.Next(2);

        var expected = new List<Op> {
            Op.Insert("llo", new AttributeMap() { { "bold", true } }),
            Op.Retain(3),
            Op.InsertEmbed(new Dictionary<string, object>() { { "embed", (object) 2 } }, new AttributeMap() { { "src", "http://quilljs.com" } }),
            Op.Delete(4)
        };
        Assert.That(iter.Rest(), Is.EqualTo(expected));

        iter.Next(3);
        expected = new List<Op> {
            Op.Retain(3),
            Op.InsertEmbed(new Dictionary<string, object>() { { "embed", (object) 2 } }, new AttributeMap() { { "src", "http://quilljs.com" } }),
            Op.Delete(4)
        };
        Assert.That(iter.Rest(), Is.EqualTo(expected));

        iter.Next(3);
        iter.Next(2);
        iter.Next(4);
        Assert.That(iter.Rest(), Is.EqualTo(new List<Op>()));
    }


}