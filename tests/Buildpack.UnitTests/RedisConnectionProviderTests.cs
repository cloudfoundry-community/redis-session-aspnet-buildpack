using Microsoft.Extensions.Configuration;
using Pivotal.Redis.Aspnet.Session.Buildpack;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.CloudFoundry.Connector.App;
using Steeltoe.CloudFoundry.Connector.Services;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Buildpack.UnitTests
{
    public class RedisConnectionProviderTests
    {
        [Fact]
        public void Test_IfImplementsIRedisConnectionProvider()
        {
            var configuration = new ConfigurationBuilder().Build();
            var provider = new RedisConnectionProvider(configuration);
            Assert.IsAssignableFrom<IRedisConnectionProvider>(provider);
        }

        [Fact]
        public void Test_IfProviderReturnsTheCorrectConnectionString_IfARedisServiveInstanceIsBounded()
        {
            var environment = @"
                                 {
                                      ""p-redis"": [
                                        {
                                          ""credentials"": {
                                            ""host"": ""10.66.32.54"",
                                            ""password"": ""4254bd8b-7f83-4a8d-8f38-8206a9d7a9f7"",
                                            ""port"": 43887
                                          },
                                          ""syslog_drain_url"": null,
                                          ""volume_mounts"": [],
                                          ""label"": ""p-redis"",
                                          ""provider"": null,
                                          ""plan"": ""shared-vm"",
                                          ""name"": ""autosource_redis_cache"",
                                          ""tags"": [
                                            ""pivotal"",
                                            ""redis""
                                          ]
                                        }
                                      ]
                                    }
                                  ";

            Environment.SetEnvironmentVariable("VCAP_SERVICES", environment);

            var configuration = new ConfigurationBuilder().AddCloudFoundry().Build();
            var provider = new RedisConnectionProvider(configuration);

            Assert.Equal("10.66.32.54:43887,password=4254bd8b-7f83-4a8d-8f38-8206a9d7a9f7,allowAdmin=false,abortConnect=true,resolveDns=false,ssl=false", provider.GetConnectionString());
        }

        [Fact]
        public void Test_IfProviderReturnsTheCorrectConnectionString_IfARedisServiveInstanceIsNotBounded()
        {
            Environment.SetEnvironmentVariable("VCAP_SERVICES", string.Empty);

            var configuration = new ConfigurationBuilder().AddCloudFoundry().Build();
            var provider = new RedisConnectionProvider(configuration);

            Assert.Equal("localhost:6379,allowAdmin=false,abortConnect=true,resolveDns=false,ssl=false", provider.GetConnectionString());
        }
    }
}
