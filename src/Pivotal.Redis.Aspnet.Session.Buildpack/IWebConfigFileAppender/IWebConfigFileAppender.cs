using System;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public interface IWebConfigFileAppender : IDisposable
    {
        void ApplyChanges();
        void SaveChanges();
    }
}
