namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public interface ICryptoGenerator
    {
        string CreateKey(int numBytes);
    }
}
