namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class BuildpackProcessor : IProcessor
    {
        private IDependencyValidator dependencyValidator;
        private IWebConfigFileAppender webConfigFileAppender;

        public BuildpackProcessor(IDependencyValidator dependencyValidator, IWebConfigFileAppender webConfigFileAppender)
        {
            this.dependencyValidator = dependencyValidator;
            this.webConfigFileAppender = webConfigFileAppender;
        }

        public void Execute()
        {
            dependencyValidator.Validate();

            using (webConfigFileAppender)
                webConfigFileAppender.ApplyChanges();
        }
    }

    public interface IProcessor
    {
        void Execute();
    }
}
