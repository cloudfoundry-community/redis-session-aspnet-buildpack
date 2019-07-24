using Moq;
using Pivotal.Redis.Aspnet.Session.Buildpack;
using Xunit;

namespace Buildpack.UnitTests
{
    public class BuildpackProcessorTests
    {
        Mock<IDependencyValidator> depValidator;
        Mock<IWebConfigFileAppender> configAppender;

        public BuildpackProcessorTests()
        {
            depValidator = new Mock<IDependencyValidator>();
            configAppender = new Mock<IWebConfigFileAppender>();
        }

        [Fact]
        public void Test_DoesExecutedValidatorAndAppender()
        {
            var processor = new BuildpackProcessor(depValidator.Object, configAppender.Object);

            processor.Execute();

            depValidator.Verify(dv => dv.Validate(), Times.Once);
            configAppender.Verify(c => c.ApplyChanges(), Times.Once);
        }
    }
}
