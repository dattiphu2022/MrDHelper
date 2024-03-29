﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDHelper.Tests
{
    [TestFixture]
    public class IListHelperTests
    {

        [Test]
        public void AddDummyItemsToMaximumCountOf_InputNullCollection_ThrowArgumentNullException()
        {
            IList<string>? Strings = null;

#pragma warning disable CS8604 // Possible null reference argument.
            Assert.Throws<ArgumentNullException>(()=> Strings.AddDummyItemsToMaximumCountOf(10, string.Empty));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Test]
        public void AddDummyItemsToMaximumCountOf_InputFinalCountSmallerThanCollectionCount_ThrowInvalidOperationException()
        {
            IList<string> Strings = new List<string>() { "",""};

            Assert.DoesNotThrow(() => Strings.AddDummyItemsToMaximumCountOf(1, string.Empty));
        }
        [Test]
        public void AddDummyItemsToMaximumCountOf_InputFinalCountIsNegative_ThrowInvalidOperationException()
        {
            IList<string> Strings = new List<string>() { "", "" };

            Assert.Throws<ArgumentException>(() => Strings.AddDummyItemsToMaximumCountOf(-1, string.Empty));
        }
        [Test]
        [TestCase(10, 10)]
        [TestCase(3, 3)]
        [TestCase(5, 5)]
        public void AddDummyItemsToMaximumCountOf_InputCount_ReturnCount(int maxCollectionCount, int numberToVerify)
        {
            IList<string> Strings = new List<string>();
            Strings.AddDummyItemsToMaximumCountOf(maxCollectionCount, string.Empty);

            var collectionCount = Strings.Count;

            Assert.That(collectionCount, Is.EqualTo(numberToVerify));
        }
        [Test]
        [TestCase("xx", "xx")]
        [TestCase(5, 5)]
        [TestCase(10f, 10f)]
        [TestCase(typeof(DummyTest), typeof(DummyTest))]
        public void AddDummyItemsToMaximumCountOf_InputObject_ReturnCompareObject<T>(T dummyItem, T itemToVerify)
        {
            IList<T> items = new List<T>();

            items.AddDummyItemsToMaximumCountOf(10, dummyItem);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.That(items.All((x) => x.Equals(itemToVerify)), Is.True);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        private class DummyTest
        {
            public DummyTest()
            {

            }
        }
    }
}
