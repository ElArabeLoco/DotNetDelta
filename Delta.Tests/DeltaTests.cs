using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace DotNetDelta.Tests;

[TestFixture]
public class DeltaTests
{

    //--------------------------------------------------------------------------------
    // Push Tests
    //--------------------------------------------------------------------------------

    [Test]
    [Category("Push")]
    public void Push_WhenEmpty()
    {
        Delta delta = new Delta();
        delta.Push(Op.Insert("test"));
        Assert.That(delta.Ops, Has.Count.EqualTo(1));
    }

    [Test]
    [Category("Push")]
    public void Push_WhenConsecutiveDeletes()
    {
        Delta delta = new Delta().Delete(2);
        delta.Push(Op.Delete(3));
        Assert.That(delta.Ops, Has.Count.EqualTo(1));
        Assert.That(delta.Ops[0], Is.EqualTo(Op.Delete(5)));
    }

    [Test]
    [Category("Push")]
    public void Push_WhenConsecutiveInserts()
    {
        Delta delta = new Delta().Insert("a");
        delta.Push(Op.Insert("b"));
        Assert.That(delta.Ops.Count, Is.EqualTo(1));
        Assert.That(delta.Ops[0], Is.EqualTo(Op.Insert("ab")));
    }

    [Test]
    [Category("Push")]
    public void Push_WhenConsecutiveInsertsWithMatchingAttrs()
    {
        Delta delta = new Delta().Insert("a", new AttributeMap() { { "bold", true } });
        delta.Push(Op.Insert("b", new AttributeMap() { { "bold", true } }));
        Assert.That(delta.Ops.Count, Is.EqualTo(1));
        Assert.That(delta.Ops[0], Is.EqualTo(Op.Insert("ab", new AttributeMap() { { "bold", true } })));
    }

    [Test]
    [Category("Push")]
    public void Push_WhenConsecutiveRetainsWithMatchingAttrs()
    {
        Delta delta = new Delta().Retain(1, new AttributeMap() { { "bold", true } });
        delta.Push(Op.Retain(3, new AttributeMap() { { "bold", true } }));
        Assert.That(delta.Ops.Count, Is.EqualTo(1));
        Assert.That(delta.Ops[0], Is.EqualTo(Op.Retain(4, new AttributeMap() { { "bold", true } })));
    }

    [Test]
    [Category("Push")]
    public void Push_WhenConsecutiveInsertsWithMismatchingAttrs()
    {
        Delta delta = new Delta().Insert("a", new AttributeMap() { { "bold", true } });
        delta.Push(Op.Insert("b"));
        Assert.That(delta.Ops, Has.Count.EqualTo(2));
    }

    [Test]
    [Category("Push")]
    public void Push_WhenConsecutiveRetainsWithMismatchingAttrs()
    {
        Delta delta = new Delta().Retain(1, new AttributeMap() { { "bold", true } });
        delta.Push(Op.Retain(3));
        Assert.That(delta.Ops, Has.Count.EqualTo(2));
    }

    [Test]
    [Category("Push")]
    public void Push_WhenConsecutiveEmbedsMatchingAttrs()
    {
        Delta delta = new Delta().Insert(new AttributeMap() { { "embed", 1 } }, new AttributeMap() { { "alt", "Description" } });
        delta.Push(Op.InsertEmbed(new AttributeMap() { { "embed", 2 } }, new AttributeMap() { { "alt", "Description" } }));
        Assert.That(delta.Ops, Has.Count.EqualTo(2));
    }



    //--------------------------------------------------------------------------------
    // Chop Tests
    //--------------------------------------------------------------------------------
    [Test]
    [Category("Chop")]
    public void Chop_WhenLastOpIsRetain_ShouldRemoveRetain()
    {
        Delta delta = new Delta().Insert("Test").Retain(4);
        Assert.That(delta.Chop(), Is.EqualTo(new Delta().Insert("Test")));
    }

    [Test]
    [Category("Chop")]
    public void Chop_WhenLastOpIsDelete()
    {
        Delta delta = new Delta().Insert("Test").Delete(4);
        Assert.That(delta.Chop(), Is.EqualTo(new Delta().Insert("Test").Delete(4)));
    }

    [Test]
    [Category("Chop")]
    public void Chop_WhenLastOpIsInsert_ShouldNotDeleteAnything()
    {
        Delta delta = new Delta().Insert("Test");
        Assert.That(delta.Chop(), Is.EqualTo(new Delta().Insert("Test")));
    }

    [Test]
    [Category("Chop")]
    public void Chop_WhenLastOpIsInsertWithAttributes_ShouldNotDeleteAnything()
    {
        Delta delta = new Delta().Insert("Test").Retain(4, new AttributeMap() { { "bold", true } });
        Assert.That(delta.Chop(), Is.EqualTo(new Delta().Insert("Test").Retain(4, new AttributeMap() { { "bold", true } })));
    }

}


[TestFixture]
public class DeltaHelpersTests
{

    Delta _delta = new();

    [SetUp]
    public void SetUp()
    {
        _delta = new Delta()
            .Insert("Hello")
            .Insert(new Dictionary<string, object>() { { "image", true } })
            .Insert("World");
    }
    //--------------------------------------------------------------------------------
    // Iteration related Tests
    //--------------------------------------------------------------------------------

    [Test]
    [Category("Iterators")]
    public void Filter()
    {
        List<Op> ops = _delta.Filter((Op op, int i) => op.insert is string);
        Assert.That(ops, Has.Count.EqualTo(2)); // Filters out second op
    }

    [Test]
    [Category("Iterators")]
    public void ForEach()
    {
        var actionDelegateMock = new Mock<Action<Op, int>>();
        _delta.ForEach(actionDelegateMock.Object);
        actionDelegateMock.Verify(action => action(It.IsAny<Op>(), It.IsAny<int>()), Times.Exactly(3));
    }

    [Test]
    [Category("Iterators")]
    public void Map()
    {
        List<string> result = _delta.Map<string>((op, i) => (string)(op.insert is string ? op.insert : ""));
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result, Is.EqualTo(new List<string>() { "Hello", "", "World" }));
    }
    
    [Test]
    [Category("Iterators")]
    public void Partition()
    {
        var (partition1, partition2) = _delta.Partition(op => op.insert is string);
        Assert.Multiple(() =>
        {
            Assert.That(partition1, Has.Count.EqualTo(2));
            Assert.That(partition2, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    [Category("Length")]
    public void Length_WithDocuments()
    {
        var delta = new Delta()
            .Insert("AB", new AttributeMap() { { "bold", true } })
            .Insert(new Dictionary<string, object>() { { "embed", 1 } });
        Assert.That(delta.Length(), Is.EqualTo(3));
    }
    
    [Test]
    [Category("Length")]
    public void Length_WithAnyTypeOfOp()
    {
        var delta = new Delta()
            .Insert("AB", new AttributeMap() { { "bold", true } })
            .Insert(new Dictionary<string, object>() { { "embed", 1 } })
            .Retain(2, new AttributeMap() {{"bold", null}})
            .Delete(1);
        Assert.That(delta.Length(), Is.EqualTo(6));
    }
    
    [Test]
    [Category("Length")]
    public void ChangeLength_WithAnyTypeOfOp()
    {
        var delta = new Delta()
            .Insert("AB", new AttributeMap() { { "bold", true } })
            .Retain(2, new AttributeMap() {{"bold", null}})
            .Delete(1);
        Assert.That(delta.ChangeLength(), Is.EqualTo(1));
    }
    
    [Test]
    [Category("Slice")]
    public void Slice_WithStartOnly()
    {
        var slicedDelta = new Delta().Retain(2).Insert("A").Slice(2);
        Assert.That(slicedDelta, Is.EqualTo(new Delta().Insert("A")));
    }
    
    [Test]
    [Category("Slice")]
    public void Slice_WithStartAndEnd_ForSingleInsert()
    {
        var slicedDelta = new Delta().Insert("0123456789").Slice(2, 7);
        Assert.That(slicedDelta, Is.EqualTo(new Delta().Insert("23456")));
    }
    
    [Test]
    [Category("Slice")]
    public void Slice_WithStartAndEnd_ForMultipleInserts()
    {
        var slicedDelta = new Delta()
            .Insert("0123", new AttributeMap() {{"bold", true}})
            .Insert("4567")
            .Slice(3, 5);
        var expected = new Delta()
            .Insert("3", new AttributeMap() { { "bold", true } })
            .Insert("4");
        Assert.That(slicedDelta, Is.EqualTo(expected));
    }
    
    [Test]
    [Category("Slice")]
    public void Slice_WithStartAndEnd_ForMultipleInsertsAndRetains()
    {
        var slicedDelta = new Delta()
            .Retain(2)
            .Insert("A", new AttributeMap() {{"bold", true}})
            .Insert("B")
            .Slice(2, 3);
        var expected = new Delta().Insert("A", new AttributeMap() { { "bold", true } });
        Assert.That(slicedDelta, Is.EqualTo(expected));
    }


    [Test]
    [Category("Slice")]
    public void Slice_WithoutParams_ShouldReturnSameDelta()
    {
        var delta = new Delta()
            .Retain(2)
            .Insert("A", new AttributeMap() {{"bold", true}})
            .Insert("B");
        Assert.That(delta.Slice(), Is.EqualTo(delta));
    }
    
    [Test]
    [Category("Slice")]
    public void Slice_WhenSplittingOp_ShouldReturnSplitOp()
    {
        var slicedDelta = new Delta()
            .Insert("AB", new AttributeMap() {{"bold", true}})
            .Insert("C")
            .Slice(1, 2);
        var expected = new Delta()
            .Insert("B", new AttributeMap() { { "bold", true } });
        Assert.That(slicedDelta, Is.EqualTo(expected));
    }
    
    [Test]
    [Category("Slice")]
    public void Slice_WhenSplittingOp()
    {
        var slicedDelta = new Delta()
            .Insert("ABC", new AttributeMap() {{"bold", true}})
            .Insert("D")
            .Slice(1, 2);
        var expected = new Delta()
            .Insert("B", new AttributeMap() { { "bold", true } });
        Assert.That(slicedDelta, Is.EqualTo(expected));
    }
    
}
