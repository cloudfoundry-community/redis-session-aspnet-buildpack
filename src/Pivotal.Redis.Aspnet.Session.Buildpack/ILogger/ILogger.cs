using System;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public interface ILogger
    {
        void WriteLog(string message);
        void WriteError(string message, Exception exception = null);
    }
}
