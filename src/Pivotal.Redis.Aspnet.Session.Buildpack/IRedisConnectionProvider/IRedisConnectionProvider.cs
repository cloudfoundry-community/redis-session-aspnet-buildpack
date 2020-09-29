namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public interface IRedisConnectionProvider
    {
        string GetConnectionString();
    }
}