using System;
using System.IO;
using System.Xml;
using Xunit;
using Pivotal.Redis.Aspnet.Session.Buildpack;

namespace IntegrationTests
{
    public class RedisSessionBuildpackTests : IDisposable
    {
        string expectedDll = Path.Combine(Environment.CurrentDirectory, "Microsoft.Web.RedisSessionStateProvider.dll");
        private readonly RedisSessionBuildpack builpack;

        public RedisSessionBuildpackTests()
        {
            builpack = Program.GetBuildpackInstance();

        }

        #region IDisposable Support
        private bool _disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (File.Exists("web.config"))
                    {
                        File.Delete("web.config");
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        [Fact]
        public void Test_Soup2Nuts_HappyPath()
        {
            //arrange
            var expectedSessionXml = "<providers><add name=\"RedisSessionStateStore\" type=\"Microsoft.Web.Redis.RedisSessionStateProvider\" connectionString=\"localhost:6379,allowAdmin=false,abortConnect=true,resolveDns=false,ssl=false\" /></providers>";
            File.WriteAllText(expectedDll, ""); //creating a dummy dependency dll (assuming the user installed the dependency nuget packages)

            // act
            builpack.Run(new[] { "supply", "", "", "", "0" });

            // assert
            var xml = new XmlDocument();
            xml.Load("web.config");

            var actualSessionXml = xml.SelectSingleNode("//configuration/system.web/sessionState").InnerXml;
            Assert.Equal(expectedSessionXml, actualSessionXml);

            var actualMachineKeyXml = xml.SelectSingleNode("//configuration/system.web/machineKey").OuterXml;
            Assert.NotEmpty(actualMachineKeyXml);
        }
    }
}
