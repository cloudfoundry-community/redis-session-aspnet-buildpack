using Moq;
using Pivotal.Redis.Aspnet.Session.Buildpack;
using Xunit;

namespace Buildpack.UnitTests
{
    public class RedisSessionBuildpackTests
    {
        Mock<ILogger> logger;
        IOptions options;
        Mock<IProcessor> processor;

        public RedisSessionBuildpackTests()
        {
            logger = new Mock<ILogger>();
            options = new ApplicationOptions();
            processor = new Mock<IProcessor>();
        }

        [Fact]
        public void Test_IfDerivedFrom()
        {
            var buildpack = new RedisSessionBuildpack(logger.Object, options, processor.Object);
            Assert.IsAssignableFrom<SupplyBuildpack>(buildpack);
        }
    }
}
