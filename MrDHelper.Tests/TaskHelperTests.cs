using MrDHelper.TaskHelper;

namespace MrDHelper.Tests
{
    [TestFixture]
    public class TaskHelperTests
    {
        private Task<T> GetInput<T>(T input)
        {
            return Task.FromResult(input);
        }

        [Test]
        [TestCase((int)10, (int)10, typeof(int))]
        [TestCase((double)10, (double)10, typeof(double))]
        [TestCase("10", "10", typeof(string))]
        public void TaskHelper_InputT_ReturnT<T>(T input, T result, Type type)
        {
            var helperResult = TaskHelper.TaskHelper.RunSync(() => GetInput(input));

            Assert.That(helperResult, Is.EqualTo(result));
            Assert.That(helperResult.GetType(), Is.EqualTo(type));
        }
    }
}