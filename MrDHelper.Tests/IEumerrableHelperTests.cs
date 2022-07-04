using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDHelper.Tests
{
    [TestFixture]
    public class IEumerrableHelperTests
    {

        [Test]
        public void ForEach_NormalInput_ShouldExecuteActionNormaly()
        {
            IEnumerable<int> ints = new[] { 1, 2, 3 };
            var sum = 0;
            ints.ForEach((i) => sum += i);

            Assert.That(sum, Is.EqualTo(ints.Sum()));
        }

        [Test]
        public void ForEach_CollectionNull_SkipAction()
        {
            IEnumerable<int>? ints = null;
            var sum = 0;
            ints.ForEach((i) => sum += i);

            Assert.That(sum, Is.EqualTo(0));
            Assert.DoesNotThrow(() => ints.ForEach(x => sum += x));
        }

        [Test]
        public void ForEach_CollectionNull_ShouldThrowArgumentNullException()
        {
            IEnumerable<int>? ints = null;
            var sum = 0;

            Assert.Throws<ArgumentNullException>(() => ints.ForEach((i) => sum += i, shouldThrowException: true));
        }
        [Test]
        public void ForEach_AlltionNull_SkipAction()
        {
            IEnumerable<int> ints = new[] { 1, 2, 3 };
            var sum = 0;

            Action<int>? action = null; 

            Assert.That(sum, Is.EqualTo(0));
#pragma warning disable CS8604 // Possible null reference argument.
            Assert.DoesNotThrow(() => ints.ForEach(action));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        [Test]
        public void ForEach_ActionNull_ShouldThrowArgumentNullException()
        {
            IEnumerable<int>? ints = null;

            Action<int>? action = null;

#pragma warning disable CS8604 // Possible null reference argument.
            Assert.Throws<ArgumentNullException>(() => ints.ForEach(action, shouldThrowException: true));
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
