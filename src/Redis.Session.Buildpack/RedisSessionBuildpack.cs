using Microsoft.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.IO;
using System.Linq;

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
            Console.WriteLine("================================================================================");
            Console.WriteLine("=================== Redis Session Buildpack execution started ==================");
            Console.WriteLine("================================================================================");


            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddCloudFoundry().Build();

            var configFileFullPath = Path.Combine(buildPath, "web.config");

            if (!File.Exists(configFileFullPath))
                Environment.Exit(0);

            var dir = new DirectoryInfo(buildPath);
            if (dir.EnumerateFiles("Microsoft.Web.RedisSessionStateProvider.dll", SearchOption.AllDirectories).ToList().Count == 0)
            {
                Console.Error.WriteLine("-----> **ERROR** Could not find assembly 'Microsoft.Web.RedisSessionStateProvider.dll' or one of its dependencies, make sure to install the nuget package 'Microsoft.Web.RedisSessionStateProvider'");
                Environment.Exit(-1);
            }

            using (var configAppender = new WebConfigFileAppender(configFileFullPath, configuration))
            {
                configAppender.ApplyMachineKeySectionChanges();
                configAppender.ApplySessionStateSectionChanges();
            }

            Console.WriteLine("================================================================================");
            Console.WriteLine("=================== Redis Session Buildpack execution completed ================");
            Console.WriteLine("================================================================================");
        }
    }
}
