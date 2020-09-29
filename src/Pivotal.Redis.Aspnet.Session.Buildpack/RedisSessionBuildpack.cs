using System;
using System.IO;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{
    public class RedisSessionBuildpack : SupplyBuildpack
    {
        private readonly ILogger logger;
        private readonly IOptions options;
        private readonly IProcessor processor;

        public RedisSessionBuildpack(ILogger logger, 
                                        IOptions options, 
                                        IProcessor processor)
        {
            this.logger = logger;
            this.options = options;
            this.processor = processor;
        }

        protected override bool Detect(string buildPath)
        {
            if (string.IsNullOrWhiteSpace(buildPath))
                buildPath = Environment.CurrentDirectory;

            return File.Exists(Path.Combine(buildPath, "web.config"));
        }

        protected override void Apply(string buildPath, string cachePath, string depsPath, int index)
        {
            try
            {
                logger.WriteLog("================================================================================");
                logger.WriteLog("=================== Redis Session Buildpack execution started ==================");
                logger.WriteLog("================================================================================");

                if (string.IsNullOrWhiteSpace(buildPath))
                    buildPath = Environment.CurrentDirectory;

                logger.WriteLog($"-----> Using buildPath {buildPath}");

                InitializeConfigAndExecuteProcessor(buildPath);

                logger.WriteLog("================================================================================");
                logger.WriteLog("=================== Redis Session Buildpack execution completed ================");
                logger.WriteLog("================================================================================");
            }
            catch (Exception exception)
            {
                logger.WriteError($"-----> **ERROR** Buildpack execution failed with exception, {exception}");
                Environment.Exit(-1);
            }
        }

        private void InitializeConfigAndExecuteProcessor(string buildPath)
        {
            options.BuildPath = buildPath;
            options.WebConfigFilePath = Path.Combine(buildPath, "web.config");
            processor.Execute();
        }
    }
}
