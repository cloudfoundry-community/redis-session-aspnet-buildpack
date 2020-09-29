using System;
using System.IO;
using System.Linq;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class DependencyValidator : IDependencyValidator
    {
        private readonly IOptions options;
        private readonly ILogger logger;

        public DependencyValidator(IOptions options, ILogger logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public void Validate()
        {
            var dir = new DirectoryInfo(options.BuildPath);
            if (dir.EnumerateFiles("Microsoft.Web.RedisSessionStateProvider.dll", SearchOption.AllDirectories).ToList().Count == 0)
            {
                var error = "-----> **ERROR** Could not find assembly 'Microsoft.Web.RedisSessionStateProvider.dll' or one of its dependencies, make sure to install the nuget package 'Microsoft.Web.RedisSessionStateProvider'";
                logger.WriteError(error);
                throw new Exception(error);
            }
        }
    }
}
