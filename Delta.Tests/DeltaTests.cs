namespace DotNetDelta.Tests;

[TestFixture]
public class DeltaTests {

    [Test]
    public void Test_Push_Into_Empty()
    {
        Delta delta = new Delta();
        delta.Push(Op.Insert("test"));
        Assert.AreEqual(1, delta.Ops.Count);
    }

    [Test]
    public void Test_Push_Consecutive_Deletes()
    {
        Delta delta = new Delta().Delete(2);
        delta.Push(Op.Delete(3));
        Assert.AreEqual(1, delta.Ops.Count);
        Assert.AreEqual(Op.Delete(5), delta.Ops[0]);
    }

    [Test]
    public void Test_Push_Consecutive_Inserts()
    {
        Delta delta = new Delta().Insert("a");
        delta.Push(Op.Insert("b"));
        Assert.AreEqual(1, delta.Ops.Count);
        Assert.AreEqual(Op.Insert("ab"), delta.Ops[0]);
    }

    [Test]
    public void Test_Push_Consecutive_Inserts_Matching_Attrs()
    {
        Delta delta = new Delta().Insert("a", new AttributeMap() { { "bold", true } });
        delta.Push(Op.Insert("b", new AttributeMap() { { "bold", true } }));
        Assert.AreEqual(1, delta.Ops.Count);
        Assert.AreEqual(Op.Insert("ab", new AttributeMap() { { "bold", true } }), delta.Ops[0]);
    }

     [Test]
    public void Test_Push_Consecutive_Retains_Matching_Attrs()
    {
        Delta delta = new Delta().Retain(1, new AttributeMap() { { "bold", true } });
        delta.Push(Op.Retain(3, new AttributeMap() { { "bold", true } }));
        Assert.AreEqual(1, delta.Ops.Count);
        Assert.AreEqual(Op.Retain(4, new AttributeMap() { { "bold", true } }), delta.Ops[0]);
    }

    [Test]
    public void Test_Push_Consecutive_Inserts_Mismatching_Attrs()
    {
        Delta delta = new Delta().Insert("a", new AttributeMap() { { "bold", true } });
        delta.Push(Op.Insert("b"));
        Assert.AreEqual(2, delta.Ops.Count);
    }

    [Test]
    public void Test_Push_Consecutive_Retains_Mismatching_Attrs()
    {
        Delta delta = new Delta().Retain(1, new AttributeMap() { { "bold", true } });
        delta.Push(Op.Retain(3));
        Assert.AreEqual(2, delta.Ops.Count);
    }

    [Test]
    public void Test_Push_Consecutive_Embeds_Matching_Attrs()
    {
        Delta delta = new Delta().Insert(new AttributeMap() {{"embed", 1}}, new AttributeMap() { { "alt", "Description" } });
        delta.Push(Op.InsertEmbed(new AttributeMap() {{"embed", 2}}, new AttributeMap() { { "alt", "Description" } }));
        Assert.AreEqual(2, delta.Ops.Count);
    }
}
