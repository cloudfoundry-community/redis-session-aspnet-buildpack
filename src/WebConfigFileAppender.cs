using Microsoft.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector.Redis;
using System;
using System.Xml;

namespace Pivotal.Redis.Session.Buildpack
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
            var sessionState = xmlDoc.SelectSingleNode("//configuration/system.web/sessionState");

            if (sessionState != null)
                sessionState.RemoveAll();
            else
            {
                sessionState = xmlDoc.CreateElement("sessionState");
                xmlDoc.SelectSingleNode("//configuration/system.web").AppendChild(sessionState);
            }

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
            providerElement.Attributes.Append(providerConnStringAttribute);

            providersNode.AppendChild(providerElement);
            sessionState.AppendChild(providersNode);
        }

        public void ApplyMachineKeySectionChanges()
        {
            var machineKey = xmlDoc.SelectSingleNode("//configuration/system.web/machineKey");

            if (machineKey != null)
                machineKey.RemoveAll();
            else
            {
                machineKey = xmlDoc.CreateElement("machineKey");
                xmlDoc.SelectSingleNode("//configuration/system.web").AppendChild(machineKey);
            }

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
