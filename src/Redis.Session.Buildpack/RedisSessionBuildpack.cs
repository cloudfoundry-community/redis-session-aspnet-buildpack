using Microsoft.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.IO;

namespace Redis.Session.Buildpack
{
    public class RedisSessionBuildpack : SupplyBuildpack
    {
        
        protected override bool Detect(string buildPath)
        {
            return File.Exists(Path.Combine(buildPath, "web.config"));
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            Console.WriteLine("=== Redis Session Buildpack ===");

            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddCloudFoundry().Build();

            var configFileFullPath = Path.Combine(buildPath, "web.config");

            if (!File.Exists(configFileFullPath))
                Environment.Exit(0);

            using (var configAppender = new WebConfigFileAppender(configFileFullPath, configuration))
            {
                configAppender.ApplyMachineKeySectionChanges();
                configAppender.ApplySessionStateSectionChanges();
            }
        }
    }
}
