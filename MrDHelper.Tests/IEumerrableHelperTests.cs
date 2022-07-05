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
        #region ForEach_Synchonous_Tests

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
        public void ForEach_ActionIsNull_SkipAction()
        {
            IEnumerable<int> ints = new[] { 1, 2, 3 };
            var sum = 0;

            Action<int>? action = null;

#pragma warning disable CS8604 // Possible null reference argument.
            Assert.DoesNotThrow(() => ints.ForEach(action));
#pragma warning restore CS8604 // Possible null reference argument.
            Assert.That(sum, Is.EqualTo(0));
        }

        [Test]
        public void ForEach_ActionNull_ShouldThrowArgumentNullException()
        {
            IEnumerable<int>? ints = new[] {1,2,3};

            Action<int>? action = null;

#pragma warning disable CS8604 // Possible null reference argument.
            Assert.Throws<ArgumentNullException>(() => ints.ForEach(action, shouldThrowException: true));
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion

        #region ForEach_Asynchonous_Tests
        [Test]
        public async Task ForEachAsync_NormalInput_ShouldExecuteFuncNormaly()
        {
            var ints = new[] { 1, 2, 3 };

            int sum = 0;

            await ints.ForEachAsync(async (x) =>
            {
                sum += x;
                await Task.Delay(10);
            });

            Assert.That(sum, Is.EqualTo(6));
        }

        [Test]
        public async Task ForEachAsync_CollectionNull_SkipAction()
        {
            IEnumerable<int>? ints = null;
            var sum = 0;
            await ints.ForEachAsync(async(i) => { sum += i; await Task.Delay(10); });

            Assert.That(sum, Is.EqualTo(0));
            Assert.DoesNotThrow(() => ints.ForEach(x => sum += x));
        }

        [Test]
        public void ForEachAsync_CollectionNull_ShouldThrowArgumentNullException()
        {
            IEnumerable<int>? ints = null;
            var sum = 0;

            Assert.ThrowsAsync<ArgumentNullException>(
                async() => await ints.ForEachAsync(
                    async(i) => { sum += i; await Task.Delay(10); }, 
                    shouldThrowException: true)
                );
        }
        [Test]
        public void ForEachAsync_FuncIsNull_SkipFunc()
        {
            IEnumerable<int> ints = new[] { 1, 2, 3 };

            Assert.DoesNotThrowAsync(() => ints.ForEachAsync(null));
        }

        [Test]
        public void ForEachAsync_FuncIsNull_ShouldThrowArgumentNullException()
        {
            IEnumerable<int>? ints = new[] {1,2,3};


            Assert.ThrowsAsync<ArgumentNullException>(() => ints.ForEachAsync(null, shouldThrowException: true));
        }
        #endregion
    }
}
