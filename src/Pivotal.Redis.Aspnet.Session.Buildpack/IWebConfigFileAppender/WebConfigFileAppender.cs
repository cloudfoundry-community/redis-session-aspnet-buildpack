using System.Xml;

namespace Pivotal.Redis.Aspnet.Session.Buildpack
{

    public class WebConfigFileAppender : IWebConfigFileAppender
    {
        private bool disposedValue = false;
        private readonly IOptions options;
        private readonly ILogger logger;
        private readonly IRedisConnectionProvider connectionProvider;
        private readonly ICryptoGenerator cryptoGenerator;
        XmlDocument xmlDoc = new XmlDocument();

        public WebConfigFileAppender(IOptions options, ILogger logger, IRedisConnectionProvider connectionProvider, ICryptoGenerator cryptoGenerator)
        {
            this.options = options;
            this.logger = logger;
            this.connectionProvider = connectionProvider;
            this.cryptoGenerator = cryptoGenerator;
        }

        public void ApplyChanges()
        {
            xmlDoc.Load(options.WebConfigFilePath);
            ApplySessionStateSectionChanges();
            ApplyMachineKeySectionChanges();
        }

        private void ApplySessionStateSectionChanges()
        {
            logger.WriteLog("-----> Removing existing session configurations...");

            var sessionStates = xmlDoc.SelectNodes("//configuration/system.web/sessionState");

            for (int i = 0; i < sessionStates.Count; i++)
                sessionStates.Item(i).ParentNode.RemoveChild(sessionStates.Item(i));

            var sessionState = xmlDoc.CreateElement("sessionState");
            xmlDoc.SelectSingleNode("//configuration/system.web").AppendChild(sessionState);

            var modeAttribute = xmlDoc.CreateAttribute("mode");
            modeAttribute.Value = "Custom";
            sessionState.Attributes.Append(modeAttribute);

            var customProviderAttribute = xmlDoc.CreateAttribute("customProvider");
            customProviderAttribute.Value = "RedisSessionStateStore";
            sessionState.Attributes.Append(customProviderAttribute);

            var providersNode = xmlDoc.CreateElement("providers");

            var providerElement = xmlDoc.CreateElement("add");
            var providerNameAttribute = xmlDoc.CreateAttribute("name");
            providerNameAttribute.Value = "RedisSessionStateStore";
            providerElement.Attributes.Append(providerNameAttribute);

            var providerTypeAttribute = xmlDoc.CreateAttribute("type");
            providerTypeAttribute.Value = "Microsoft.Web.Redis.RedisSessionStateProvider";
            providerElement.Attributes.Append(providerTypeAttribute);

            var providerConnStringAttribute = xmlDoc.CreateAttribute("connectionString");
            providerConnStringAttribute.Value = connectionProvider.GetConnectionString();

            logger.WriteLog($"-----> Found redis connection '{connectionProvider.GetConnectionString()}'");

            logger.WriteLog($"-----> Creating sessionState section with the above connection string...");

            providerElement.Attributes.Append(providerConnStringAttribute);

            providersNode.AppendChild(providerElement);
            sessionState.AppendChild(providersNode);
        }

        private void ApplyMachineKeySectionChanges()
        {
            logger.WriteLog("-----> Removing existing machineKey configuration...");

            var machineKeys = xmlDoc.SelectNodes("//configuration/system.web/machineKey");

            for (int i = 0; i < machineKeys.Count; i++)
                machineKeys.Item(i).ParentNode.RemoveChild(machineKeys.Item(i));

            logger.WriteLog($"-----> Creating machineKey section with new validation, decryption keys and SHA1 validation...");

            var machineKey = xmlDoc.CreateElement("machineKey");
            xmlDoc.SelectSingleNode("//configuration/system.web").AppendChild(machineKey);

            var validationKeyAttribute = xmlDoc.CreateAttribute("validationKey");
            validationKeyAttribute.Value = cryptoGenerator.CreateKey(64);
            machineKey.Attributes.Append(validationKeyAttribute);

            var decryptionKeyAttribute = xmlDoc.CreateAttribute("decryptionKey");
            decryptionKeyAttribute.Value = cryptoGenerator.CreateKey(24);
            machineKey.Attributes.Append(decryptionKeyAttribute);

            var validationAttribute = xmlDoc.CreateAttribute("validation");
            validationAttribute.Value = "SHA1";
            machineKey.Attributes.Append(validationAttribute);
        }

        /// <summary>
        /// Saves automatically when appender is disposed
        /// </summary>
        public void SaveChanges()
        {
            xmlDoc.Save(options.WebConfigFilePath);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SaveChanges();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
