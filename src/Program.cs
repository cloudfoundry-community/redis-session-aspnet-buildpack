using Microsoft.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.IO;

namespace Pivotal.Redis.Session.Buildpack
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: Pivotal.Redis.Session.Buildpack.exe <appDir>");
                Environment.Exit(1);
            }
            var appPath = args[0];

            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddCloudFoundry().Build();

            var configFileFullPath = Path.Combine(appPath, "web.config");

            if(!File.Exists(configFileFullPath))
                Environment.Exit(0);

            using (var configAppender = new WebConfigFileAppender(configFileFullPath, configuration))
            {
                configAppender.ApplyMachineKeySectionChanges();
                configAppender.ApplySessionStateSectionChanges();
            }

        }
    }
}
