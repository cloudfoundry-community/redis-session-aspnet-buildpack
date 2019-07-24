using System;
using System.IO;
using System.Linq;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class DependencyValidator : IDependencyValidator
    {
        private readonly string buildPath;

        public DependencyValidator(string buildPath)
        {
            this.buildPath = buildPath;
        }

        public void Validate()
        {
            var dir = new DirectoryInfo(buildPath);
            if (dir.EnumerateFiles("Microsoft.Web.RedisSessionStateProvider.dll", SearchOption.AllDirectories).ToList().Count == 0)
            {
                throw new Exception("-----> **ERROR** Could not find assembly 'Microsoft.Web.RedisSessionStateProvider.dll' or one of its dependencies, make sure to install the nuget package 'Microsoft.Web.RedisSessionStateProvider'");
            }
        }
    }

    public interface IDependencyValidator
    {
        void Validate();
    }
}
