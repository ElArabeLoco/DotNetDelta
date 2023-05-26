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

        //--------------------------------------------------------------------------------
        // Invert Tests
        //--------------------------------------------------------------------------------

        [Test]
        public void Test_Invert_WhenArgumentIsNull()
        {
            AttributeMap baseAttributes = new AttributeMap();
            baseAttributes["bold"] = true;
            Assert.AreEqual(new AttributeMap(), AttributeMap.Invert(null, baseAttributes));
        }


        [Test]
        public void Test_Invert_WhenBaseIsNull()
        {
            AttributeMap appliedAttributes = new AttributeMap();
            appliedAttributes["bold"] = true;
            appliedAttributes["color"] = "red";

            AttributeMap expected = new AttributeMap();
            expected["bold"] = null;
            expected["color"] = null;

            Assert.AreEqual(expected, AttributeMap.Invert(appliedAttributes, null));
        }

        [Test]
        public void Test_Invert_WhenBothArgumentsAreNull()
        {
            Assert.AreEqual(new AttributeMap(), AttributeMap.Invert(null, null));
        }

        [Test]
        public void Test_Invert_WhenBaseWasModifiedByAnotherAttributeMapAddingAttributes()
        {
            AttributeMap baseAttributes = new AttributeMap();
            baseAttributes["italic"] = true;

            AttributeMap appliedAttributes = new AttributeMap();
            appliedAttributes["bold"] = true;

            AttributeMap expected = new AttributeMap();
            expected["bold"] = null;

            Assert.AreEqual(expected, AttributeMap.Invert(appliedAttributes, baseAttributes));
        }


        [Test]
        public void Test_Invert_WhenBaseWasModifiedByAnotherAttributeMapThatNullifiesAttribute()
        {
            AttributeMap baseAttributes = new AttributeMap();
            baseAttributes["bold"] = true;

            AttributeMap appliedAttributes = new AttributeMap();
            appliedAttributes["bold"] = null;

            AttributeMap expected = new AttributeMap();
            expected["bold"] = true;

            Assert.AreEqual(expected, AttributeMap.Invert(appliedAttributes, baseAttributes));
        }

        [Test]
        public void Test_Invert_WhenBaseWasModifiedByAnotherAttributeMapReplacingAttributes()
        {
            AttributeMap baseAttributes = new AttributeMap();
            baseAttributes["color"] = "blue";

            AttributeMap appliedAttributes = new AttributeMap();
            appliedAttributes["color"] =  "red";

            AttributeMap expected = baseAttributes;

            Assert.AreEqual(expected, AttributeMap.Invert(appliedAttributes, baseAttributes));
        }

        [Test]
        public void Test_Invert_WhenANotModifyingAttributeApplied()
        {
            AttributeMap baseAttributes = new AttributeMap();
            baseAttributes["color"] = "blue";

            AttributeMap appliedAttributes = new AttributeMap();
            appliedAttributes["color"] =  "blue";

            Assert.AreEqual(new AttributeMap(), AttributeMap.Invert(appliedAttributes, baseAttributes));
        }

        [Test]
        public void Test_Invert_WhenBaseWasModifiedByAnotherAttributeMapAddingAndRemovingAttributes()
        {
            AttributeMap baseAttributes = new AttributeMap();
            baseAttributes["font"] = "serif";
            baseAttributes["italic"] = true;
            baseAttributes["color"] = "blue";
            baseAttributes["size"] = "12px";

            AttributeMap appliedAttributes = new AttributeMap();
            appliedAttributes["bold"] = true; // add
            appliedAttributes["italic"] = null; // remove
            appliedAttributes["color"] = "red"; // replace
            appliedAttributes["size"] = "12px"; // not modified

            AttributeMap expected = new AttributeMap();
            expected["bold"] = null; 
            expected["italic"] = true;
            expected["color"] = "blue";

            Assert.AreEqual(expected, AttributeMap.Invert(appliedAttributes, baseAttributes));
        }
        

        //--------------------------------------------------------------------------------
        // Transform Tests
        //--------------------------------------------------------------------------------

        [Test]
        public void Test_Transform_WhenLeftIsNull()
        {
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(null, null));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(null, null, true));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(null, new AttributeMap()));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(null, new AttributeMap(), true));
        }

        [Test]
        public void Test_Transform_WhenRightIsNull()
        {
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(null, null));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(null, null, true));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(new AttributeMap(), null));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(new AttributeMap(), null, true));
        }

        [Test]
        public void Test_Transform_WhenBothAreNull()
        {
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(null, null));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Transform(null, null, true));
        }

        [Test]
        public void Test_Transform_WithPriority()
        {
            AttributeMap left = new AttributeMap();
            left["bold"] = true;
            left["color"] = "red";
            left["font"] = null;

            AttributeMap right = new AttributeMap();
            right["color"] = "blue";
            right["font"] = "serif";
            right["italic"] = true;

            AttributeMap expected = new AttributeMap();
            expected["italic"] = true;

            Assert.AreEqual(expected, AttributeMap.Transform(left, right, true));
        }

        [Test]
        public void Test_Transform_WithoutPriority()
        {
            AttributeMap left = new AttributeMap();
            left["bold"] = true;
            left["color"] = "red";
            left["font"] = null;

            AttributeMap right = new AttributeMap();
            right["color"] = "blue";
            right["font"] = "serif";
            right["italic"] = true;

            AttributeMap expected = right;

            Assert.AreEqual(expected, AttributeMap.Transform(left, right, false));
        }


    }
}


