using Microsoft.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.CloudFoundry.Connector.Services;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public interface IRedisConnectionProvider
    {
        string GetConnectionString();
    }

    public class RedisConnectionProvider : IRedisConnectionProvider
    {
        private readonly IConfiguration configuration;

        public RedisConnectionProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetConnectionString()
        {
            var info = configuration.GetSingletonServiceInfo<RedisServiceInfo>();
            var redisConfig = new RedisCacheConnectorOptions(configuration);

            var connectionOptions = new RedisCacheConfigurer().Configure(info, redisConfig);
            return connectionOptions.ToString();
        }
    }
}