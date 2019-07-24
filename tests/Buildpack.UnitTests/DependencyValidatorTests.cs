using Pivotal.Redis.Aspnet.Session.Buildpack;
using System;
using System.IO;
using Xunit;

namespace Buildpack.UnitTests
{
    public class DependencyValidatorTests
    {
        string expectedDll = Path.Combine(Environment.CurrentDirectory, "assembly", "Microsoft.Web.RedisSessionStateProvider.dll");

        public DependencyValidatorTests()
        {
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "assembly"));
        }

        [Fact]
        public void Test_IfDoesNotThrowExecptionIfProviderDllsExist()
        {
            File.WriteAllText(expectedDll, "");
            var validator = new DependencyValidator(Path.Combine(Environment.CurrentDirectory, "assembly"));
            validator.Validate();
        }

        [Fact]
        public void Test_IfThrowsExecptionIfRedisSessionStateProviderIsMissing()
        {
            File.WriteAllText(expectedDll, "");

            File.Delete(expectedDll);
            var validator = new DependencyValidator(Path.Combine(Environment.CurrentDirectory, "assembly"));
            Assert.Throws<Exception>(()=>validator.Validate());
        }
    }
}
