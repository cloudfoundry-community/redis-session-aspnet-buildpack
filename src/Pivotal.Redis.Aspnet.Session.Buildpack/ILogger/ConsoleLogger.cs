using System;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class ConsoleLogger : ILogger
    {
        public void WriteError(string message, Exception exception = null)
        {
            Console.Error.WriteLine($"{message}. Exception: {exception}");
        }

        public void WriteLog(string message)
        {
            Console.WriteLine(message);
        }
    }
}
