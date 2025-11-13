using MrDHelper.GenericHelper;
using System;

namespace MrDHelper.Tests
{
    [TestFixture]
    public class GenericHelperTests
    {
        [Test]
        [TestCase("", false)]
        [TestCase(5, false)]
        public void IsNull_ShoulPassAllTestCase<T>(T input, bool resultToVerify)
        {
            var isNull = input.IsNull();

            Assert.That(isNull, Is.EqualTo(resultToVerify));
        }
        [Test]
        public void IsNull_InputObjectNull_ReturnTrue()
        {
            object? obj = null;
            
            var isNull = obj.IsNull();

            Assert.That(isNull, Is.True);
        }
        [Test]
        public void IsNull_InputNull_ReturnTrue()
        {
            string? stringToTest = null;

            var isNull = stringToTest.IsNull();

            Assert.That(isNull, Is.True);
        }

        [Test]
        public void IsNull_InputNotNull_ReturnFalse()
        {
            string? stringToTest = "x";
            string? stringToTest2 = string.Empty;

            var isNull = stringToTest.IsNull();
            var isNull2 = stringToTest2.IsNull();
            Assert.Multiple(() =>
            {
                Assert.That(isNull, Is.False);
                Assert.That(isNull2, Is.False);
            });
        }

        [Test]
        public void NotNull_InputNull_ReturnFalse()
        {
            string? stringToTest = "x";
            string? stringToTest2 = string.Empty;

            var isNull = stringToTest.NotNull();
            var isNull2 = stringToTest2.NotNull();
            Assert.Multiple(() =>
            {
                Assert.That(isNull, Is.True);
                Assert.That(isNull2, Is.True);
            });
        }
    }
}