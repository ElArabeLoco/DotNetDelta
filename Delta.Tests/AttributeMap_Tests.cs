namespace DotNetDelta.Tests
{
    [TestFixture]
    public class AttributeMap_Tests
    {
        [Test]
        public void Test_Compose_WhenArgumentsAreNull()
        {
            AttributeMap nonNull = new AttributeMap();
            nonNull["key"] = "value";
            
            Assert.AreEqual(nonNull, AttributeMap.Compose(null, nonNull));
            Assert.AreEqual(nonNull, AttributeMap.Compose(nonNull, null));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Compose(null, null));
        }

        [Test]
        public void Test_Compose_WhenArgumentsAreEmpty()
        {
            AttributeMap nonEmpty = new AttributeMap();
            nonEmpty["key"] = "value";
            AttributeMap empty = new AttributeMap();
            
            Assert.AreEqual(nonEmpty, AttributeMap.Compose(empty, nonEmpty));
            Assert.AreEqual(nonEmpty, AttributeMap.Compose(nonEmpty, empty));
            Assert.AreEqual(empty, AttributeMap.Compose(empty, empty));
        }

        [Test]
        public void Test_Compose_AddsMissing()
        {
            AttributeMap first = new AttributeMap();
            first["bold"] = true;
            first["color"] = "red";
            AttributeMap second = new AttributeMap();
            second["italic"] = true;

            AttributeMap expected = new AttributeMap();
            expected["bold"] = true;
            expected["color"] = "red";
            expected["italic"] = true;
            
            Assert.AreEqual(expected, AttributeMap.Compose(first, second));
        }

        [Test]
        public void Test_Compose_OverwritesExisting()
        {
            AttributeMap first = new AttributeMap();
            first["bold"] = true;
            first["color"] = "red";
            
            AttributeMap second = new AttributeMap();
            second["color"] = "blue";

            AttributeMap expected = new AttributeMap();
            expected["bold"] = true;
            expected["color"] = "blue";
            
            Assert.AreEqual(expected, AttributeMap.Compose(first, second));
        }

        [Test]
        public void Test_Compose_RemovesExisting()
        {
            AttributeMap first = new AttributeMap();
            first["bold"] = true;
            first["color"] = "red";
            
            AttributeMap second = new AttributeMap();
            second["color"] = null;

            AttributeMap expected = new AttributeMap();
            expected["bold"] = true;
            
            Assert.AreEqual(expected, AttributeMap.Compose(first, second));
        }
    }
}


