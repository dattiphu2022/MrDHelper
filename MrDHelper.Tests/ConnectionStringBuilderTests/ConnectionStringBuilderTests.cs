
namespace MrDHelper.Tests
{
    public class ConnectionStringBuilderTests
    {

        [Test]
        public void ShouldReturnNullWithEmptyBuilder()
        {
            //arrange
            var stringRendered = ConnectionStringBuilder.Empty().NullIfEmpty();

            //assert
            Assert.That(stringRendered, Is.Null);
        }

        [Test]
        public void ShouldBulidConditionalPropertyClasses()
        {
            //arrange
            var hasTwo = false;
            var hasThree = true;
            Func<bool> hasFive = () => false;

            //act
            var stringRendered = new ConnectionStringBuilder()
                            .AddProperty("item-two", when: hasTwo)
                            .AddProperty("item-three", when: hasThree)
                            .AddProperty("item-four")
                            .AddProperty("item-five", when: hasFive)
                            .Build();
            //assert
            Assert.That(stringRendered, Is.EqualTo("item-three;item-four;"));
        }
        [Test]
        public void ShouldBuildConditionalConnectionBuilderProperties()
        {
            //arrange
            var hasTwo = false;
            var hasThree = true;
            Func<bool> hasFive = () => false;

            //act
            var stringRendered = new ConnectionStringBuilder()
                            .AddProperty("item-two", when: hasTwo)
                            .AddProperty(new ConnectionStringBuilder()
                                            .AddProperty("item-foo", false)
                                            .AddProperty("item-sub-three"),
                                            when: hasThree)
                            .AddProperty("item-four")
                            .AddProperty("item-five", when: hasFive)
                            .Build();
            //assert
            Assert.That(stringRendered, Is.EqualTo("item-sub-three;item-four;"));
        }
        [Test]
        public void ShouldBulidEmptyProperties()
        {
            //arrange
            var shouldShow = false;

            //act
            var stringRendered = new ConnectionStringBuilder()
                            .AddProperty("some-class", shouldShow)
                            .Build();
            //assert
            Assert.That(stringRendered, Is.Empty);
        }

    }
}