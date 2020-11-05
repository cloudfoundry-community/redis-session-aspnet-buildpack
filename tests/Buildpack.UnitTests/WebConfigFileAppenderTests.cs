using Moq;
using Pivotal.Redis.Aspnet.Session.Buildpack;
using System;
using System.IO;
using XmlDiffLib;
using Xunit;

namespace Buildpack.UnitTests
{
    public class WebConfigFileAppenderTests
    {
        string expectedConfigPath = Path.Combine(Environment.CurrentDirectory, "ConfigFiles", "Expected.config");
        string givenConfigPathTemplate = Path.Combine(Environment.CurrentDirectory, "ConfigFiles", "Given{0}.config");
        ILogger logger;
        IOptions options;

        public WebConfigFileAppenderTests()
        {
            logger = new ConsoleLogger();
            options = new ApplicationOptions();
        }

        [Fact]
        public void Test_IfImplementsIWebConfigFileAppender()
        {
            options.WebConfigFilePath = string.Format(givenConfigPathTemplate, "WithoutSession");
            var appender = new WebConfigFileAppender(options, logger, new RedisConnectionProviderStub(), new CryptoGeneratorStub());
            Assert.IsAssignableFrom<IWebConfigFileAppender>(appender);
        }

        [Fact]
        public void Test_IfImplementsIDisposable()
        {
            options.WebConfigFilePath = string.Format(givenConfigPathTemplate, "WithoutSession");
            var appender = new WebConfigFileAppender(options, logger, new RedisConnectionProviderStub(), new CryptoGeneratorStub());
            Assert.IsAssignableFrom<IDisposable>(appender);
        }

        [Fact]
        public void Test_IfRedisConnectionProviderAndCryptoGeneratorAreInvokedAsNeeded()
        {
            options.WebConfigFilePath = string.Format(givenConfigPathTemplate, "WithoutSession");

            var provider = new Mock<IRedisConnectionProvider>();
            var generator = new Mock<ICryptoGenerator>();

            using (var appender = new WebConfigFileAppender(options, logger, provider.Object, generator.Object))
                appender.ApplyChanges();

            provider.Verify(p => p.GetConnectionString(), Times.Exactly(2));
            generator.Verify(g => g.CreateKey(24), Times.Once);
            generator.Verify(g => g.CreateKey(64), Times.Once);
        }

        [Fact]
        public void Test_AppliesNecessaryConfigurationSessionStateAndMachineConfig_IfNoSessionOrMachineKeySectionIsProvided()
        {
            var configFilePath = string.Format(givenConfigPathTemplate, "WithoutSession");

            var appendedConfigFilePath = string.Format(givenConfigPathTemplate, "WithoutSession_Test");

            AppendAndCompare(configFilePath, appendedConfigFilePath);
        }

        [Fact]
        public void Test_AppliesNecessaryConfigurationSessionStateAndMachineConfig_IfSessionIsProvidedButNoMachineKeyProvided()
        {
            var configFilePath = string.Format(givenConfigPathTemplate, "WithSessionNoMachineKey");
            var appendedConfigFilePath = string.Format(givenConfigPathTemplate, "WithSessionNoMachineKey_Test");

            AppendAndCompare(configFilePath, appendedConfigFilePath);
        }

        [Fact]
        public void Test_AppliesNecessaryConfigurationSessionStateAndMachineConfig_IfSessionIsProvidedAndMachineKeyProvided()
        {
            var configFilePath = string.Format(givenConfigPathTemplate, "WithSessionWithMachineKey");
            var appendedConfigFilePath = string.Format(givenConfigPathTemplate, "WithSessionWithMachineKey_Test");
            AppendAndCompare(configFilePath, appendedConfigFilePath);
        }

        private void AppendAndCompare(string configFilePath, string appendedConfigFilePath)
        {
            File.Copy(configFilePath, appendedConfigFilePath, true);
            
            options.WebConfigFilePath = appendedConfigFilePath;
            
            using (var appender = new WebConfigFileAppender(options, logger, new RedisConnectionProviderStub(), new CryptoGeneratorStub()))
                appender.ApplyChanges();
            
            var expectedWebConfig = File.ReadAllText(expectedConfigPath);
            var appendedWebConfig = File.ReadAllText(appendedConfigFilePath);
            
            var diff = new XmlDiff(expectedWebConfig, appendedWebConfig);
            
            diff.CompareDocuments(new XmlDiffOptions() { IgnoreAttributeOrder = true, IgnoreCase = true, TrimWhitespace = true });
            
            Assert.Empty(diff.DiffNodeList);
        }
    }

    internal class CryptoGeneratorStub : ICryptoGenerator
    {
        public string CreateKey(int numBytes)
        {
            switch (numBytes)
            {
                case 24:
                    return "7359ABC0085C6E14DC3DCBBBB5FE4D4CC51B98926BC2B575B3F6788213130D3B";
                case 64:
                    return "7D545A9FDC7FD39F808593FACD44D1E0A3281A815A8D296DACA0471DA0C936BE6A6F2D867B45135F0E01F31C3DD11CE887351DBBC586DBC973581EEB01DBC639";
                default:
                    throw new Exception($"Stub is not setup for {numBytes} number of bytes");
            }
        }
    }

    internal class RedisConnectionProviderStub : IRedisConnectionProvider
    {
        public string GetConnectionString()
        {
            return "localhost:6379,allowAdmin=false,abortConnect=true,resolveDns=false,ssl=false";
        }
    }
}
