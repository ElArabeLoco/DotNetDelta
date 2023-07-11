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
        Delta delta = new Delta().Insert(new AttributeMap() { { "embed", 1 } },
            new AttributeMap() { { "alt", "Description" } });
        delta.Push(Op.InsertEmbed(new AttributeMap() { { "embed", 2 } },
            new AttributeMap() { { "alt", "Description" } }));
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
        Assert.That(delta.Chop(),
            Is.EqualTo(new Delta().Insert("Test").Retain(4, new AttributeMap() { { "bold", true } })));
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
            .Retain(2, new AttributeMap() { { "bold", null } })
            .Delete(1);
        Assert.That(delta.Length(), Is.EqualTo(6));
    }

    [Test]
    [Category("Length")]
    public void ChangeLength_WithAnyTypeOfOp()
    {
        var delta = new Delta()
            .Insert("AB", new AttributeMap() { { "bold", true } })
            .Retain(2, new AttributeMap() { { "bold", null } })
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
            .Insert("0123", new AttributeMap() { { "bold", true } })
            .Insert("4567")
            .Slice(3, 5);
        var expected = new Delta()
            .Insert("3", new AttributeMap() { { "bold", true } })
            .Insert("4");
        Assert.That(slicedDelta, Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Slice")]
    public void Slice_WithStartAndEnd_ForMultipleInsertsAndRetains()
    {
        var slicedDelta = new Delta()
            .Retain(2)
            .Insert("A", new AttributeMap() { { "bold", true } })
            .Insert("B")
            .Slice(2, 3);
        var expected = new Delta().Insert("A", new AttributeMap() { { "bold", true } });
        Assert.That(slicedDelta, Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }


    [Test]
    [Category("Slice")]
    public void Slice_WithoutParams_ShouldReturnSameDelta()
    {
        var delta = new Delta()
            .Retain(2)
            .Insert("A", new AttributeMap() { { "bold", true } })
            .Insert("B");
        Assert.That(delta.Slice(), Is.EqualTo(delta));
        Console.WriteLine("Exp: " + delta.ToJson(true));
    }

    [Test]
    [Category("Slice")]
    public void Slice_WhenSplittingOp_ShouldReturnSplitOp()
    {
        var slicedDelta = new Delta()
            .Insert("AB", new AttributeMap() { { "bold", true } })
            .Insert("C")
            .Slice(1, 2);
        var expected = new Delta()
            .Insert("B", new AttributeMap() { { "bold", true } });
        Assert.That(slicedDelta, Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Slice")]
    public void Slice_WhenSplittingOp()
    {
        var slicedDelta = new Delta()
            .Insert("ABC", new AttributeMap() { { "bold", true } })
            .Insert("D")
            .Slice(1, 2);
        var expected = new Delta()
            .Insert("B", new AttributeMap() { { "bold", true } });
        Assert.That(slicedDelta, Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }
}

[TestFixture]
public class DeltaComposeTests
{
    [Test]
    [Category("Compose")]
    public void Compose_WithInserts()
    {
        var a = new Delta().Insert("A");
        var b = new Delta().Insert("B");
        var expected = new Delta().Insert("B").Insert("A");
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithInsertAndRetain()
    {
        var a = new Delta().Insert("A");
        var b = new Delta().Retain(1, new AttributeMap { { "bold", true }, { "color", "red" }, { "font", null } });
        var expected = new Delta().Insert("A", new AttributeMap { { "bold", true }, { "color", "red" } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithInsertAndDelete()
    {
        var a = new Delta().Insert("A");
        var b = new Delta().Delete(1);
        var expected = new Delta();
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithDeleteAndInsert()
    {
        var a = new Delta().Delete(1);
        var b = new Delta().Insert("B");
        var expected = new Delta().Insert("B").Delete(1);
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithDeleteAndRetain()
    {
        var a = new Delta().Delete(1);
        var b = new Delta().Retain(1, new AttributeMap { { "bold", true }, { "color", "red" } });
        var expected = new Delta()
            .Delete(1)
            .Retain(1, new AttributeMap { { "bold", true }, { "color", "red" } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithDeleteAndDelete()
    {
        var a = new Delta().Delete(1);
        var b = new Delta().Delete(1);
        var expected = new Delta().Delete(2);
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithRetainAndInsert()
    {
        var a = new Delta().Retain(1, new AttributeMap { { "color", "blue" } });
        var b = new Delta().Insert("B");
        var expected = new Delta().Insert("B").Retain(1, new AttributeMap { { "color", "blue" } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithRetainAndRetain()
    {
        var a = new Delta().Retain(1, new AttributeMap { { "color", "blue" } });
        var b = new Delta().Retain(1, new AttributeMap { { "bold", true }, { "color", "red" }, { "font", null } });
        var expected =
            new Delta().Retain(1, new AttributeMap { { "bold", true }, { "color", "red" }, { "font", null } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithRetainAndDelete()
    {
        var a = new Delta().Retain(1, new AttributeMap { { "color", "blue" } });
        var b = new Delta().Delete(1);
        var expected = new Delta().Delete(1);
        Assert.That(a.Compose(b), Is.EqualTo(expected));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithInsertInMiddleOfText()
    {
        var a = new Delta().Insert("Hello");
        var b = new Delta().Retain(3).Insert("X");
        var expected = new Delta().Insert("HelXlo");
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithInsertAndDeleteOrdering()
    {
        var a = new Delta().Insert("Hello");
        var b = new Delta().Insert("Hello");
        var insertFirst = new Delta().Retain(3).Insert("X").Delete(1);
        var deleteFirst = new Delta().Retain(3).Delete(1).Insert("X");
        var expected = new Delta().Insert("HelXo");
        Assert.Multiple(() =>
        {
            Assert.That(a.Compose(insertFirst), Is.EqualTo(expected));
            Assert.That(b.Compose(deleteFirst), Is.EqualTo(expected));
        });
    }

    [Test]
    [Category("Compose")]
    public void Compos_WithInsertEmbed()
    {
        var a = new Delta().Insert(new Dictionary<string, object> { { "embed", 1 } },
            new AttributeMap { { "image", "http://quilljs.com" } });
        var b = new Delta().Retain(1, new AttributeMap { { "alt", "logo" } });

        var expected = new Delta().Insert(
            new Dictionary<string, object> { { "embed", 1 } },
            new AttributeMap { { "image", "http://quilljs.com" }, { "alt", "logo" } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WithRetainEmbed()
    {
        var a = new Delta().Retain(
            new Dictionary<string, object> { { "figure", true } },
            new AttributeMap { { "src", "http://quilljs.com/image.png" } });
        var b = new Delta().Retain(1, new AttributeMap { { "alt", "logo" } });
        var expected = new Delta().Retain(
            new Dictionary<string, object> { { "figure", true } },
            new AttributeMap { { "alt", "logo" }, { "src", "http://quilljs.com/image.png" } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WhenDeleteEntireText()
    {
        var a = new Delta().Retain(4).Insert("Hello");
        var b = new Delta().Delete(9);
        var expected = new Delta().Delete(4);
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WhenRetainMoreThanTextLength_ShouldReturnTheInsertedTextSoFar()
    {
        var a = new Delta().Insert("Hello");
        var b = new Delta().Retain(10);
        var expected = new Delta().Insert("Hello");
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WhenRetainEmptyEmbed_ShouldReturnTheInsertedEmbed()
    {
        var a = new Delta().Insert(new Dictionary<string, object> { { "embed", 1 } });
        var b = new Delta().Retain(1);
        var expected = new Delta().Insert(new Dictionary<string, object> { { "embed", 1 } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WhenRemovingAttribute()
    {
        var a = new Delta().Insert("A", new AttributeMap { { "bold", true } });
        var b = new Delta().Retain(1, new AttributeMap { { "bold", null } });
        var expected = new Delta().Insert("A");
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_WhenRemovingEmbedAttributes()
    {
        var a = new Delta().Insert(new Dictionary<string, object> { { "embed", 2 } },
            new AttributeMap { { "bold", true } });
        var b = new Delta().Retain(1, new AttributeMap { { "bold", null } });
        var expected = new Delta().Insert(new Dictionary<string, object> { { "embed", 2 } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_DeltasAndAttributesAreImmutable()
    {
        var attr1 = new AttributeMap { { "bold", true } };
        var attr2 = new AttributeMap { { "bold", true } };
        var a1 = new Delta().Insert("Test", attr1);
        var a2 = new Delta().Insert("Test", attr1);
        var b1 = new Delta().Retain(1, new AttributeMap { { "color", "red" } }).Delete(2);
        var b2 = new Delta().Retain(1, new AttributeMap { { "color", "red" } }).Delete(2);
        var expected = new Delta()
            .Insert("T", new AttributeMap { { "color", "red" }, { "bold", true } })
            .Insert("t", attr1);
        Assert.Multiple(() =>
        {
            Assert.That(a1.Compose(b1), Is.EqualTo(expected));
            Assert.That(a1, Is.EqualTo(a2));
            Assert.That(b1, Is.EqualTo(b2));
            Assert.That(attr1, Is.EqualTo(attr2));
        });
    }

    [Test]
    [Category("Compose")]
    public void Compose_WhenRetainStartOptimization()
    {
        var a = new Delta()
            .Insert("A", new AttributeMap { { "bold", true } })
            .Insert("B")
            .Insert("C", new AttributeMap { { "bold", true } })
            .Delete(1);
        var b = new Delta().Retain(3).Insert("D");
        var expected = new Delta()
            .Insert("A", new AttributeMap { { "bold", true } })
            .Insert("B")
            .Insert("C", new AttributeMap { { "bold", true } })
            .Insert("D")
            .Delete(1);
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_RetainStartOptimizationSplit()
    {
        var a = new Delta()
            .Insert("A", new AttributeMap { { "bold", true } })
            .Insert("B")
            .Insert("C", new AttributeMap { { "bold", true } })
            .Retain(5) // Forward 5 positions
            .Delete(1); // Delete 1 position
        var b = new Delta().Retain(4).Insert("D");
        var expected = new Delta()
            .Insert("A", new AttributeMap { { "bold", true } })
            .Insert("B")
            .Insert("C", new AttributeMap { { "bold", true } })
            .Retain(1)
            .Insert("D") // This is inserted between the 5 positions retain, which causes that retain to split
            .Retain(4)
            .Delete(1);
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    public void Compose_RetainEndOptimization()
    {
        var a = new Delta()
            .Insert("A", new AttributeMap { { "bold", true } })
            .Insert("B")
            .Insert("C", new AttributeMap { { "bold", true } });
        var b = new Delta().Delete(1);
        var expected = new Delta()
            .Insert("B")
            .Insert("C", new AttributeMap { { "bold", true } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

    [Test]
    [Category("Compose")]
    public void Compose_RetainEndOptimizationJoin()
    {
        var a = new Delta()
            .Insert("A", new AttributeMap { { "bold", true } })
            .Insert("B")
            .Insert("C", new AttributeMap { { "bold", true } })
            .Insert("D")
            .Insert("E", new AttributeMap { { "bold", true } })
            .Insert("F");
        var b = new Delta().Retain(1).Delete(1);
        var expected = new Delta()
            .Insert("AC", new AttributeMap { { "bold", true } })
            .Insert("D")
            .Insert("E", new AttributeMap { { "bold", true } })
            .Insert("F");
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }
}

[TestFixture]
[Category("Compose")]
public class DeltaComposeWithEmbedsTests
{

    [SetUp]
    public void Setup()
    {
        // Delta.RegisterEmbed("embed", new CustomEmbedHandler());
        Delta.RegisterEmbed(new DefaultEmbedHandler("embed"));
    }

    [TearDown]
    public void TearDown()
    {
        Delta.UnregisterEmbed("embed");
    }

    [Test]
    public void Compose_WithARetainOfEmbedWithNumber()
    {
        var a = new Delta()
            .Insert(new AttributeMap { { "embed", new AttributeMap { { "attr", "a" } } } });
        var b = new Delta()
            .Retain(1, new AttributeMap { { "bold", true } });
        var expected = new Delta()
            .Insert(new AttributeMap { { "embed", new AttributeMap { { "attr", "a" } } } },
                new AttributeMap { { "bold", true } });
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }
    
    [Test]
    public void Compose_WithARetainOfNumberWithEmbed()
    {
        var a = new Delta()
            .Retain(10, new AttributeMap { { "bold", true } });
        var b = new Delta()
            .Retain(new AttributeMap { { "embed", new AttributeMap { { "attr", "a" } } } });
        var expected = new Delta()
            .Retain(new AttributeMap { { "embed", new AttributeMap { { "attr", "a" } } } }, 
                new AttributeMap { { "bold", true } })
            .Retain(9, new AttributeMap { { "bold", true } });
        
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
        
    }
    
    [Test]
    public void Compose_RetainEmbedWithAnEmbed_EmbedsShouldComposeToo()
    {
        var a = new Delta()
            .Insert(new AttributeMap { { "embed", new AttributeMap { { "attr1", 1 } } } });
        var b = new Delta()
            .Retain(new AttributeMap { { "embed", new AttributeMap { { "attr2", 2 } } } });
        var expected = new Delta()
            .Insert(new AttributeMap { { "embed", new AttributeMap { { "attr1", 1 }, { "attr2", 2 } } } });
        
        Assert.That(a.Compose(b), Is.EqualTo(expected));
        Console.WriteLine("Exp: " + expected.ToJson(true));
    }

}

[TestFixture]
[Category("JSON")]

public class DeltaJSONSerializationDeserialization
{
    [Test]
    public void ToJson_WithInsertsWithAttributes_Deletes_Retains()
    {
        var delta = new Delta()
            .Insert("Hello", new AttributeMap { { "bold", true } })
            .Insert("World")
            .Delete(3)
            .Retain(4, new AttributeMap { { "color", "#fff" } });
        var expected = @"{""ops"":[{""insert"":""Hello"",""attributes"":{""bold"":true}},{""insert"":""World""},{""delete"":3},{""retain"":4,""attributes"":{""color"":""#fff""}}]}";
        Console.WriteLine("Exp: " + expected);
        Console.WriteLine("Act: " + delta.ToJson());
        Assert.That(delta.ToJson(), Is.EqualTo(expected));
    } 
    
    [Test]
    public void ToJson_NullAttributeValuesAreNotIgnored()
    {
        var delta = new Delta()
            .Insert("Hello", new AttributeMap { { "bold", null } })
            .Insert("World")
            .Delete(3)
            .Retain(4, new AttributeMap { { "color", "#fff" } });
        var expected = @"{""ops"":[{""insert"":""Hello"",""attributes"":{""bold"":null}},{""insert"":""World""},{""delete"":3},{""retain"":4,""attributes"":{""color"":""#fff""}}]}";
        Console.WriteLine("Exp: " + expected);
        Console.WriteLine("Act: " + delta.ToJson());
        Assert.That(delta.ToJson(), Is.EqualTo(expected));
    } 
    
    
    [Test]
    public void FromJson_Basic()
    {
        const string delta = @"
{
    ""ops"": [
        {
            ""insert"": ""Testing how this ""
        },
        {
            ""attributes"": {
                ""bold"": true
            },
            ""insert"": ""delta""
        },
        {
            ""insert"": ""\ngets ""
        },
        {
            ""attributes"": {
                ""size"": ""42px"",
                ""underline"": true,
                ""italic"": true
            },
            ""insert"": ""parsed""
        },
        {
            ""insert"": ""\ninto ""
        },
        {
            ""insert"": ""JSON"",
            ""attributes"": {
                ""size"": ""98px"",
                ""font"": ""verdana""
            }
        },
        {
            ""insert"": ""\n""
        }
    ]
}";
        
        var expected = new Delta()
            .Insert("Testing how this ")
            .Insert("delta", new AttributeMap { { "bold", true } })
            .Insert("\ngets ")
            .Insert("parsed", new AttributeMap { { "size", "42px" }, { "underline", true }, { "italic", true } })
            .Insert("\ninto ")
            .Insert("JSON", new AttributeMap { { "size", "98px" }, { "font", "verdana" } })
            .Insert("\n");

        Assert.That(Delta.FromJson(delta), Is.EqualTo(expected));
    } 
}

