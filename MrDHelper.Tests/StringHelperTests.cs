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

        [Test]
        [TestCase("")]
        [TestCase("1")]
        [TestCase("123")]
        [TestCase("123$")]
        [TestCase("123$3434")]
        [TestCase(@"123$3434-09l'ksdf\we[}/?")]
        public void EncryptDecryptShouldWorkNormaly(string input)
        {
            string password = "password";

            var encryped = input.EncryptToBase64(password);
            var decrypted = encryped.DecryptFromBase64(password);

            Assert.That(decrypted, Is.EqualTo(input));
        }
    }
}