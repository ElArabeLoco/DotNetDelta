namespace DotNetDelta.Tests
{
    [TestFixture]
    public class AttributeMapTests
    {
        //--------------------------------------------------------------------------------
        // Compose Tests
        //--------------------------------------------------------------------------------
        [Test]
        public void Test_Compose_WhenArgumentsAreNull()
        {
            AttributeMap nonNull = new AttributeMap() {{"key", "value"}};
            Assert.AreEqual(nonNull, AttributeMap.Compose(null, nonNull));
            Assert.AreEqual(nonNull, AttributeMap.Compose(nonNull, null));
            Assert.AreEqual(new AttributeMap(), AttributeMap.Compose(null, null));
        }

        [Test]
        public void Test_Compose_WhenArgumentsAreEmpty()
        {
            AttributeMap nonEmpty = new AttributeMap() {{"key", "value"}};
            AttributeMap empty = new AttributeMap();
            Assert.AreEqual(nonEmpty, AttributeMap.Compose(empty, nonEmpty));
            Assert.AreEqual(nonEmpty, AttributeMap.Compose(nonEmpty, empty));
            Assert.AreEqual(empty, AttributeMap.Compose(empty, empty));
        }

        [Test]
        public void Test_Compose_AddsMissing()
        {
            AttributeMap first = new AttributeMap() {{"bold", true}, {"color", "red"}};
            AttributeMap second = new AttributeMap() {{"italic", true}};
            AttributeMap expected = new AttributeMap() {{"bold", true}, {"color", "red"}, {"italic", true}};
            Assert.AreEqual(expected, AttributeMap.Compose(first, second));
        }

        [Test]
        public void Test_Compose_OverwritesExisting()
        {
            AttributeMap first = new AttributeMap() {{"bold", true}, {"color", "red"}};
            AttributeMap second = new AttributeMap() {{"color", "blue"}};
            AttributeMap expected = new AttributeMap() {{"bold", true}, {"color", "blue"}};
            Assert.AreEqual(expected, AttributeMap.Compose(first, second));
        }

        [Test]
        public void Test_Compose_RemovesExisting()
        {
            AttributeMap first = new AttributeMap() {{"bold", true}, {"color", "red"}};            
            AttributeMap second = new AttributeMap() {{"color", null}};
            AttributeMap expected = new AttributeMap() {{"bold", true}};
            Assert.AreEqual(expected, AttributeMap.Compose(first, second));
        }

        //--------------------------------------------------------------------------------
        // Diff Tests
        //--------------------------------------------------------------------------------



        [Test]
        public void Test_Diff_WhenLeftIsNull()
        {
            AttributeMap format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            Assert.AreEqual(format, AttributeMap.Diff(null, format));
        }

        [Test]
        public void Test_Diff_WhenRightIsNull()
        {
            AttributeMap format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            AttributeMap expected = new AttributeMap() {{"bold", null}, {"color", null}};
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
            AttributeMap format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            Assert.AreEqual(new AttributeMap(), AttributeMap.Diff(format, format));
        }

        [Test]
        public void Test_Diff_WhenAddingFormat()
        {
            AttributeMap format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            AttributeMap added = new AttributeMap() {{"bold", true}, {"italic", true}, {"color", "red"}};

            AttributeMap expected = new AttributeMap() {{"italic", true}};
           
            Assert.AreEqual(expected, AttributeMap.Diff(format, added));
        }

        [Test]
        public void Test_Diff_WhenRemovingFormat()
        {
            AttributeMap format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            AttributeMap removing = new AttributeMap() {{"bold", true}};
            AttributeMap expected = new AttributeMap() {{"color", null}}; 
            Assert.AreEqual(expected, AttributeMap.Diff(format, removing));
        }

        [Test]
        public void Test_Diff_WhenOverwritingFormat()
        {
            AttributeMap format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            AttributeMap overwriting = new AttributeMap() {{"bold", true}, {"color", "blue"}};
            AttributeMap expected = new AttributeMap() {{ "color", "blue" }};
            Assert.AreEqual(expected, AttributeMap.Diff(format, overwriting));
        }

        //--------------------------------------------------------------------------------
        // Invert Tests
        //--------------------------------------------------------------------------------

        [Test]
        public void Test_Invert_WhenArgumentIsNull()
        {
            AttributeMap baseAttributes = new AttributeMap() {{"bold", true}};
            Assert.AreEqual(new AttributeMap(), AttributeMap.Invert(null, baseAttributes));
        }


        [Test]
        public void Test_Invert_WhenBaseIsNull()
        {
            AttributeMap appliedAttributes = new AttributeMap() {{"bold", true}, {"color", "red"}};
            AttributeMap expected = new AttributeMap() {{"bold", null}, {"color", null}};
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
            AttributeMap baseAttributes = new AttributeMap() {{"italic", true}};
            AttributeMap appliedAttributes = new AttributeMap() {{"bold", true}};
            AttributeMap expected = new AttributeMap() {{"bold", null}};
            Assert.AreEqual(expected, AttributeMap.Invert(appliedAttributes, baseAttributes));
        }


        [Test]
        public void Test_Invert_WhenBaseWasModifiedByAnotherAttributeMapThatNullifiesAttribute()
        {
            AttributeMap baseAttributes = new AttributeMap() {{"bold", true}};
            AttributeMap appliedAttributes = new AttributeMap() {{"bold", null}};
            AttributeMap expected = new AttributeMap() {{"bold", true}};
            Assert.AreEqual(expected, AttributeMap.Invert(appliedAttributes, baseAttributes));
        }

        [Test]
        public void Test_Invert_WhenBaseWasModifiedByAnotherAttributeMapReplacingAttributes()
        {
            AttributeMap baseAttributes = new AttributeMap() {{"color", "blue"}};
            AttributeMap appliedAttributes = new AttributeMap() {{"color", "red"}};
            AttributeMap expected = baseAttributes;
            Assert.AreEqual(expected, AttributeMap.Invert(appliedAttributes, baseAttributes));
        }

        [Test]
        public void Test_Invert_WhenANotModifyingAttributeApplied()
        {
            AttributeMap baseAttributes = new AttributeMap() {{"color", "blue"}};
            AttributeMap appliedAttributes = new AttributeMap() {{"color", "blue"}};
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
            AttributeMap left = new AttributeMap() {{"bold", true}, {"color", "red"}, {"font", null}}; 
            AttributeMap right = new AttributeMap() {{"color", "blue"}, {"font", "serif"}, {"italic", true}};
            AttributeMap expected = new AttributeMap() {{"italic", true}};
            Assert.AreEqual(expected, AttributeMap.Transform(left, right, true));
        }

        [Test]
        public void Test_Transform_WithoutPriority()
        {
            AttributeMap left = new AttributeMap() {{"bold", true}, {"color", "red"}, {"font", null}};
            AttributeMap right = new AttributeMap() {{"color", "blue"}, {"font", "serif"}, {"italic", true}};
            AttributeMap expected = right;
            Assert.AreEqual(expected, AttributeMap.Transform(left, right, false));
        }


    }
}


