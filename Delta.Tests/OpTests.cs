namespace DotNetDelta.Tests;

[TestFixture]
public class OpTests
{


    [Test]
    public void Test_Length_Delete()
    {
        Op op = new Op();
        op.delete = 5;
        Assert.That(Op.Length(op), Is.EqualTo(5));
    }

    [Test]
    public void Test_Length_Retain()
    {
        Op op = new Op();
        op.retain = 2;
        Assert.That(Op.Length(op), Is.EqualTo(2));
    }

    [Test]
    public void Test_Length_InsertText()
    {
        Op op = new Op();
        op.insert = "text";
        Assert.That(Op.Length(op), Is.EqualTo(4));
    }

    [Test]
    public void Test_Length_InsertEmbed()
    {
        Op op = new Op
        {
            insert = new Dictionary<string, string>() { { "image", "http://example.com/image.png" } }
        };
        Assert.That(Op.Length(op), Is.EqualTo(1));
    }

}