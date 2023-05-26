namespace DotNetDelta.Tests
{
    [TestFixture]
    public class AttributeMap_Tests
    {
        //--------------------------------------------------------------------------------
        // Compose Tests
        //--------------------------------------------------------------------------------
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

        //--------------------------------------------------------------------------------
        // Diff Tests
        //--------------------------------------------------------------------------------



        [Test]
        public void Test_Diff_WhenLeftIsNull()
        {
            AttributeMap format = new AttributeMap();
            format["bold"] = true;
            format["color"] = "red";
            
            Assert.AreEqual(format, AttributeMap.Diff(null, format));
        }

        [Test]
        public void Test_Diff_WhenRightIsNull()
        {
            AttributeMap format = new AttributeMap();
            format["bold"] = true;
            format["color"] = "red";

            AttributeMap expected = new AttributeMap();
            expected["bold"] = null;
            expected["color"] = null;
            
            Assert.AreEqual(expected, AttributeMap.Diff(format, null));
        }


        [Test]
        public void Test_Diff_WhenBothArgumentsAreNull()
        {
           
            Assert.AreEqual(new AttributeMap(), AttributeMap.Diff(null, null));
        }


        [Test]
        public void Test_Diff_WhenBothArgumentsAreSame()
        {
            AttributeMap format = new AttributeMap();
            format["bold"] = true;
            format["color"] = "red";
           
            Assert.AreEqual(new AttributeMap(), AttributeMap.Diff(format, format));
        }

        [Test]
        public void Test_Diff_WhenAddingFormat()
        {
            AttributeMap format = new AttributeMap();
            format["bold"] = true;
            format["color"] = "red";

            AttributeMap added = new AttributeMap();
            added["bold"] = true;
            added["italic"] = true;
            added["color"] = "red";

            AttributeMap expected = new AttributeMap();
            expected["italic"] = true;
           
            Assert.AreEqual(expected, AttributeMap.Diff(format, added));
        }

        [Test]
        public void Test_Diff_WhenRemovingFormat()
        {
            AttributeMap format = new AttributeMap();
            format["bold"] = true;
            format["color"] = "red";

            AttributeMap removing = new AttributeMap();
            removing["bold"] = true;

            AttributeMap expected = new AttributeMap(); 
            expected["color"] = null;
           
            Assert.AreEqual(expected, AttributeMap.Diff(format, removing));
        }

        [Test]
        public void Test_Diff_WhenOverwritingFormat()
        {
            AttributeMap format = new AttributeMap();
            format["bold"] = true;
            format["color"] = "red";

            AttributeMap overwriting = new AttributeMap();
            overwriting["bold"] = true;
            overwriting["color"] = "blue";

            AttributeMap expected = new AttributeMap();
            expected["color"] = "blue";
           
            Assert.AreEqual(expected, AttributeMap.Diff(format, overwriting));
        }

    }
}


