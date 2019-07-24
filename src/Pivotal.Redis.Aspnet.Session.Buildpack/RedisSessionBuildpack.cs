using Microsoft.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class RedisSessionBuildpack : SupplyBuildpack
    {
        protected override bool Detect(string buildPath)
        {
            return File.Exists(Path.Combine(buildPath, "web.config"));
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            try
            {
                Console.WriteLine("================================================================================");
                Console.WriteLine("=================== Redis Session Buildpack execution started ==================");
                Console.WriteLine("================================================================================");

                var webConfigPath = Path.Combine(buildPath, "web.config");

                if (!File.Exists(webConfigPath))
                {
                    Console.WriteLine("-----> **WARNING** Web.config file not found, so skipping ececution...");
                    return;
                }

                BuildProcessor(webConfigPath, buildPath).Execute();

                Console.WriteLine("================================================================================");
                Console.WriteLine("=================== Redis Session Buildpack execution completed ================");
                Console.WriteLine("================================================================================");
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"-----> **ERROR** Buildpack execution failed with exception, {exception}");
                Environment.Exit(-1);
            }
        }

        private IProcessor BuildProcessor(string webConfigPath, string buildPath)
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddCloudFoundry().Build();

            var dependencyValidator = new DependencyValidator(buildPath);
            var redisConnectionProvider = new RedisConnectionProvider(configuration);
            var cryptoGenerator = new CryptoGenerator();
            var configFileAppender = new WebConfigFileAppender(webConfigPath, redisConnectionProvider, cryptoGenerator);

            return new BuildpackProcessor(dependencyValidator, configFileAppender);
        }
    }
}
