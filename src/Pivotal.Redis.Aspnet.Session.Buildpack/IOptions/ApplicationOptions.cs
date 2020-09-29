using System;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class ApplicationOptions : IOptions
    {
        public string BuildPath { get; set; }
        public string WebConfigFilePath { get; set; }
    }
}
