namespace DotNetDelta.Tests;

[TestFixture]
public class OpTests
{


    [Test]
    public void Test_Length_Delete()
    {
        Op op = new Op();
        op.delete = 5;
        Assert.AreEqual(5, Op.Length(op));
    }

    [Test]
    public void Test_Length_Retain()
    {
        Op op = new Op();
        op.retain = 2;
        Assert.AreEqual(2, Op.Length(op));
    }

    [Test]
    public void Test_Length_InsertText()
    {
        Op op = new Op();
        op.insert = "text";
        Assert.AreEqual(4, Op.Length(op));
    }

    [Test]
    public void Test_Length_InsertEmbed()
    {
        Op op = new Op();
        op.insert = new Dictionary<string, string>() { { "image", "http://example.com/image.png" } };
        Assert.AreEqual(1, Op.Length(op));
    }

}