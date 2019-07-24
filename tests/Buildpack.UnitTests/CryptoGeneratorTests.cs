using Pivotal.Redis.Aspnet.Session.Buildpack;
using System;
using System.Linq;
using Xunit;

namespace Buildpack.UnitTests
{
    public class CryptoGeneratorTests
    {
        [Fact]
        public void Test_IfImplementsICryptoGenerator()
        {
            var generator = new CryptoGenerator();
            Assert.IsAssignableFrom<ICryptoGenerator>(generator);
        }

        [InlineData(24)]
        [InlineData(64)]
        [Theory]
        public void Test_IfProducesHexStringKeyForGivenNoOfBytes(int byteLength)
        {
            var generator = new CryptoGenerator();
            var key = generator.CreateKey(byteLength);

            Assert.Equal(byteLength * 2, key.Length);
            Assert.True(IsHexString(key));
        }

        private bool IsHexString(string text)
        {
            for (int i = 0; i < text.Length; i++)
                if (!Uri.IsHexDigit(Convert.ToChar(text.Substring(i, 1))))
                    return false;
            return true;
        }
    }
}
