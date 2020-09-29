
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class Program
    {
        static int Main(string[] args)
        {
            return GetBuildpackInstance().Run(args);
        }

        public static ServiceProvider RegisterServices()
        {
            var configuration = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .AddCloudFoundry()
               .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddSingleton<IOptions, ApplicationOptions>()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>()
                .AddSingleton<IWebConfigFileAppender, WebConfigFileAppender>()
                .AddSingleton<ICryptoGenerator, CryptoGenerator>()
                .AddSingleton<IDependencyValidator, DependencyValidator>()
                .AddSingleton<IProcessor, BuildpackProcessor>()
                .AddSingleton<RedisSessionBuildpack>()
                .BuildServiceProvider();
            return serviceProvider;
        }

        public static RedisSessionBuildpack GetBuildpackInstance()
        {
            return RegisterServices().GetService<RedisSessionBuildpack>();
        }
    }
}