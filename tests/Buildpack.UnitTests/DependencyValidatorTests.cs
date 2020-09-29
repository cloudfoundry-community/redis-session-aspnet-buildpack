using Pivotal.Redis.Aspnet.Session.Buildpack;
using System;
using System.IO;
using Xunit;

namespace Buildpack.UnitTests
{
    public class DependencyValidatorTests
    {
        string expectedDll = Path.Combine(Environment.CurrentDirectory, "assembly", "Microsoft.Web.RedisSessionStateProvider.dll");
        ILogger logger;
        IOptions options;

        public DependencyValidatorTests()
        {
            var buildPath = Path.Combine(Environment.CurrentDirectory, "assembly");
            Directory.CreateDirectory(buildPath);
            options = new ApplicationOptions { BuildPath = buildPath };
            logger = new ConsoleLogger();
        }

        [Fact]
        public void Test_IfDoesNotThrowExecptionIfProviderDllsExist()
        {
            File.WriteAllText(expectedDll, "");
            var validator = new DependencyValidator(options, logger);
            validator.Validate();
        }

        [Fact]
        public void Test_IfThrowsExecptionIfRedisSessionStateProviderIsMissing()
        {
            File.WriteAllText(expectedDll, "");

            File.Delete(expectedDll);
            var validator = new DependencyValidator(options, logger);
            Assert.Throws<Exception>(() => validator.Validate());
        }
    }
}
