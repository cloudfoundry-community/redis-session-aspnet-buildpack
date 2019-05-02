using Microsoft.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector.Redis;
using System;
using System.Xml;

namespace Redis.Session.Buildpack
{
    internal class WebConfigFileAppender : IDisposable
    {
        private bool disposedValue = false;
        private readonly string fileFullPath;
        private readonly IConfiguration configuration;
        XmlDocument xmlDoc = new XmlDocument();

        public WebConfigFileAppender(string fileFullPath, IConfiguration configuration)
        {
            this.fileFullPath = fileFullPath;
            this.configuration = configuration;
            xmlDoc.Load(fileFullPath);
        }
        public void ApplySessionStateSectionChanges()
        {
            Console.WriteLine("Removing existing session configurations...");

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
            providerConnStringAttribute.Value = new RedisCacheConnectorOptions(configuration).ToString();

            Console.WriteLine($"Found redis connection '{providerConnStringAttribute.Value}'");

            providerElement.Attributes.Append(providerConnStringAttribute);

            providersNode.AppendChild(providerElement);
            sessionState.AppendChild(providersNode);
        }

        public void ApplyMachineKeySectionChanges()
        {
            Console.WriteLine("Removing existing machineKey configuration...");

            var machineKeys = xmlDoc.SelectNodes("//configuration/system.web/machineKey");

            for (int i = 0; i < machineKeys.Count; i++)
                machineKeys.Item(i).ParentNode.RemoveChild(machineKeys.Item(i));

            Console.WriteLine($"Creating machineKey section with new validation, decryption keys and SHA1 validation...");

            var machineKey = xmlDoc.CreateElement("machineKey");
            xmlDoc.SelectSingleNode("//configuration/system.web").AppendChild(machineKey);

            var validationKeyAttribute = xmlDoc.CreateAttribute("validationKey");
            validationKeyAttribute.Value = CryptoGenerator.CreateKey(64);
            machineKey.Attributes.Append(validationKeyAttribute);

            var decryptionKeyAttribute = xmlDoc.CreateAttribute("decryptionKey");
            decryptionKeyAttribute.Value = CryptoGenerator.CreateKey(24);
            machineKey.Attributes.Append(decryptionKeyAttribute);

            var validationAttribute = xmlDoc.CreateAttribute("validation");
            validationAttribute.Value = "SHA1";
            machineKey.Attributes.Append(validationAttribute);
        }

        public void SaveChanges()
        {
            xmlDoc.Save(fileFullPath);
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
