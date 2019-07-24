
namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class Program
    {
        static int Main(string[] args)
        {
            return new RedisSessionBuildpack().Run(args);
        }
    }
}