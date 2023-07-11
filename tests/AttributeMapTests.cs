namespace DotNetDelta.Tests
{
    [TestFixture]
    public class AttributeMapTests
    {
        //--------------------------------------------------------------------------------
        // Compose Tests
        //--------------------------------------------------------------------------------
        [Test]
        public void Compose_WhenArgumentsAreNull_ShouldReturnEmptyAttributes()
        {
            var nonNull = new AttributeMap() {{"key", "value"}};
            Assert.Multiple(() =>
            {
                Assert.That(AttributeMap.Compose(null, nonNull), Is.EqualTo(nonNull));
                Assert.That(AttributeMap.Compose(nonNull, null), Is.EqualTo(nonNull));
                Assert.That(AttributeMap.Compose(null, null), Is.EqualTo(new AttributeMap()));
            });
        }

        [Test]
        public void Compose_WhenArgumentsAreEmpty_ShouldReturnEmptyAttributes()
        {
            var nonEmpty = new AttributeMap() {{"key", "value"}};
            var empty = new AttributeMap();
            Assert.Multiple(() =>
            {
                Assert.That(AttributeMap.Compose(empty, nonEmpty), Is.EqualTo(nonEmpty));
                Assert.That(AttributeMap.Compose(nonEmpty, empty), Is.EqualTo(nonEmpty));
                Assert.That(AttributeMap.Compose(empty, empty), Is.EqualTo(empty));
            });
        }

        [Test]
        public void Compose_WhenAddingMissing_ShouldReturnMergedAttributes()
        {
            var first = new AttributeMap() {{"bold", true}, {"color", "red"}};
            var second = new AttributeMap() {{"italic", true}};
            var expected = new AttributeMap() {{"bold", true}, {"color", "red"}, {"italic", true}};
            Assert.That(AttributeMap.Compose(first, second), Is.EqualTo(expected));
        }

        [Test]
        public void Compose_WhenAddingExisting_OverwritesExisting()
        {
            var first = new AttributeMap() {{"bold", true}, {"color", "red"}};
            var second = new AttributeMap() {{"color", "blue"}};
            var expected = new AttributeMap() {{"bold", true}, {"color", "blue"}};
            Assert.That(AttributeMap.Compose(first, second), Is.EqualTo(expected));
        }

        [Test]
        public void Compose_WhenRemovesExisting_ShouldReturnAttributesWithoutTheRemoved()
        {
            var first = new AttributeMap() {{"bold", true}, {"color", "red"}};            
            var second = new AttributeMap() {{"color", null}};
            var expected = new AttributeMap() {{"bold", true}};
            Assert.That(AttributeMap.Compose(first, second), Is.EqualTo(expected));
        }

        //--------------------------------------------------------------------------------
        // Diff Tests
        //--------------------------------------------------------------------------------



        [Test]
        public void Diff_WhenLeftIsNull_ShouldReturnRight()
        {
            var format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            Assert.That(AttributeMap.Diff(null, format), Is.EqualTo(format));
        }

        [Test]
        public void Diff_WhenRightIsNull_ShouldReturnLeftAttributesWithNullValues()
        {
            var format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            var expected = new AttributeMap() {{"bold", null}, {"color", null}};
            Assert.That(AttributeMap.Diff(format, null), Is.EqualTo(expected));
        }


        [Test]
        public void Diff_WhenBothArgumentsAreNull_ShouldReturnEmptyAttributes()
        {
            Assert.That(AttributeMap.Diff(null, null), Is.EqualTo(new AttributeMap()));
        }


        [Test]
        public void Diff_WhenBothArgumentsAreSame_ShouldReturnEmptyAttributes()
        {
            var format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            Assert.That(AttributeMap.Diff(format, format), Is.EqualTo(new AttributeMap()));
        }

        [Test]
        public void Diff_WhenKeepingSomeAttributeValuesAndAddingNewOnes_ShouldReturnNewAttributesOnly()
        {
            var format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            var added = new AttributeMap() {{"bold", true}, {"italic", true}, {"color", "red"}};

            var expected = new AttributeMap() {{"italic", true}};
           
            Assert.That(AttributeMap.Diff(format, added), Is.EqualTo(expected));
        }

        [Test]
        public void Diff_WhenRemovingAnAttribute_ShouldReturnRemovedAttributeWithNullValue()
        {
            var format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            var removing = new AttributeMap() {{"bold", true}};
            var expected = new AttributeMap() {{"color", null}}; 
            Assert.That(AttributeMap.Diff(format, removing), Is.EqualTo(expected));
        }

        [Test]
        public void Diff_WhenOverwritingFormat_ShouldReturnOnlyTheOverritenAttribute()
        {
            var format = new AttributeMap() {{"bold", true}, {"color", "red"}};
            var overwriting = new AttributeMap() {{"bold", true}, {"color", "blue"}};
            var expected = new AttributeMap() {{ "color", "blue" }};
            Assert.That(AttributeMap.Diff(format, overwriting), Is.EqualTo(expected));
        }

        //--------------------------------------------------------------------------------
        // Invert Tests
        //--------------------------------------------------------------------------------

        [Test]
        public void Invert_WhenArgumentIsNull()
        {
            var baseAttributes = new AttributeMap() {{"bold", true}};
            Assert.That(AttributeMap.Invert(null, baseAttributes), Is.EqualTo(new AttributeMap()));
        }


        [Test]
        public void Invert_WhenBaseIsNull()
        {
            var appliedAttributes = new AttributeMap() {{"bold", true}, {"color", "red"}};
            var expected = new AttributeMap() {{"bold", null}, {"color", null}};
            Assert.That(AttributeMap.Invert(appliedAttributes, null), Is.EqualTo(expected));
        }

        [Test]
        public void Invert_WhenBothArgumentsAreNull()
        {
            Assert.That(AttributeMap.Invert(null, null), Is.EqualTo(new AttributeMap()));
        }

        [Test]
        public void Invert_WhenBaseWasModifiedByAnotherAttributeMapAddingAttributes()
        {
            var baseAttributes = new AttributeMap() {{"italic", true}};
            var appliedAttributes = new AttributeMap() {{"bold", true}};
            var expected = new AttributeMap() {{"bold", null}};
            Assert.That(AttributeMap.Invert(appliedAttributes, baseAttributes), Is.EqualTo(expected));
        }


        [Test]
        public void Invert_WhenBaseWasModifiedByAnotherAttributeMapThatNullifiesAttribute()
        {
            var baseAttributes = new AttributeMap() {{"bold", true}};
            var appliedAttributes = new AttributeMap() {{"bold", null}};
            var expected = new AttributeMap() {{"bold", true}};
            Assert.That(AttributeMap.Invert(appliedAttributes, baseAttributes), Is.EqualTo(expected));
        }

        [Test]
        public void Invert_WhenBaseWasModifiedByAnotherAttributeMapReplacingAttributes()
        {
            var baseAttributes = new AttributeMap() {{"color", "blue"}};
            var appliedAttributes = new AttributeMap() {{"color", "red"}};
            var expected = baseAttributes;
            Assert.That(AttributeMap.Invert(appliedAttributes, baseAttributes), Is.EqualTo(expected));
        }

        [Test]
        public void Invert_WhenANotModifyingAttributeApplied()
        {
            var baseAttributes = new AttributeMap() {{"color", "blue"}};
            var appliedAttributes = new AttributeMap() {{"color", "blue"}};
            Assert.That(AttributeMap.Invert(appliedAttributes, baseAttributes), Is.EqualTo(new AttributeMap()));
        }

        [Test]
        public void Invert_WhenBaseWasModifiedByAnotherAttributeMapAddingAndRemovingAttributes()
        {
            var baseAttributes = new AttributeMap();
            baseAttributes["font"] = "serif";
            baseAttributes["italic"] = true;
            baseAttributes["color"] = "blue";
            baseAttributes["size"] = "12px";

            var appliedAttributes = new AttributeMap();
            appliedAttributes["bold"] = true; // add
            appliedAttributes["italic"] = null; // remove
            appliedAttributes["color"] = "red"; // replace
            appliedAttributes["size"] = "12px"; // not modified

            var expected = new AttributeMap();
            expected["bold"] = null; 
            expected["italic"] = true;
            expected["color"] = "blue";

            Assert.That(AttributeMap.Invert(appliedAttributes, baseAttributes), Is.EqualTo(expected));
        }
        

        //--------------------------------------------------------------------------------
        // Transform Tests
        //--------------------------------------------------------------------------------

        [Test]
        public void Transform_WhenLeftIsNull()
        {
            Assert.Multiple(() =>
            {
                Assert.That(AttributeMap.Transform(null, null), Is.EqualTo(new AttributeMap()));
                Assert.That(AttributeMap.Transform(null, null, true), Is.EqualTo(new AttributeMap()));
                Assert.That(AttributeMap.Transform(null, new AttributeMap()), Is.EqualTo(new AttributeMap()));
                Assert.That(AttributeMap.Transform(null, new AttributeMap(), true), Is.EqualTo(new AttributeMap()));
            });
        }

        [Test]
        public void Transform_WhenRightIsNull()
        {
            Assert.Multiple(() =>
            {
                Assert.That(AttributeMap.Transform(null, null), Is.EqualTo(new AttributeMap()));
                Assert.That(AttributeMap.Transform(null, null, true), Is.EqualTo(new AttributeMap()));
                Assert.That(AttributeMap.Transform(new AttributeMap(), null), Is.EqualTo(new AttributeMap()));
                Assert.That(AttributeMap.Transform(new AttributeMap(), null, true), Is.EqualTo(new AttributeMap()));
            });
        }

        [Test]
        public void Transform_WhenBothAreNull()
        {
            Assert.Multiple(() =>
            {
                Assert.That(AttributeMap.Transform(null, null), Is.EqualTo(new AttributeMap()));
                Assert.That(AttributeMap.Transform(null, null, true), Is.EqualTo(new AttributeMap()));
            });
        }

        [Test]
        public void Transform_WithPriority()
        {
            var left = new AttributeMap() {{"bold", true}, {"color", "red"}, {"font", null}}; 
            var right = new AttributeMap() {{"color", "blue"}, {"font", "serif"}, {"italic", true}};
            var expected = new AttributeMap() {{"italic", true}};
            Assert.That(AttributeMap.Transform(left, right, true), Is.EqualTo(expected));
        }

        [Test]
        public void Transform_WithoutPriority()
        {
            var left = new AttributeMap() {{"bold", true}, {"color", "red"}, {"font", null}};
            var right = new AttributeMap() {{"color", "blue"}, {"font", "serif"}, {"italic", true}};
            var expected = right;
            Assert.That(AttributeMap.Transform(left, right, false), Is.EqualTo(expected));
        }


    }
}


