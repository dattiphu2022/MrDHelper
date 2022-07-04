using System;

namespace MrDHelper.Tests
{
    [TestFixture]
    public class StringHelperTests
    {
        [Test]
        [TestCase(null, null)]
        [TestCase("", "d41d8cd98f00b204e9800998ecf8427e")]
        [TestCase("asdfasdkf;j09a83j4a,mdsfv89xcv", "cf90a51119574a777bbac525fc0019bd")]
        [TestCase("345*&8743508(*8364-__0834", "7c69e60616e6247a6ceb4ed14d934993")]
        public void GetMd5_InputNull_ReturnNull(string? input, string? md5ToVerify)
        {
            var caculatedMd5 = input.GetMd5();

            Assert.That(caculatedMd5, Is.EqualTo(md5ToVerify));
        }
    }
}