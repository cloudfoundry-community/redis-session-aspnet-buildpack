using System;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public interface IOptions
    {
        string BuildPath { get; set; }
        string WebConfigFilePath { get; set; }
    }
}
