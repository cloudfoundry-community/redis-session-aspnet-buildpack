using Microsoft.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml;

namespace sample_test_session_section
{
    public class Global : HttpApplication
    {
        const string SESSION_STATE_SECTION = "system.web/sessionState";
        const string MACHINE_KEY_SECTION = "system.web/machineKey";
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            //var webConfig = Path.Combine(@"C:\Users\AlfusInigoJaganathan\source\repos\sample_test_session_section\sample_test_session_section", "web.config");

            //var configBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddEnvironmentVariables().AddCloudFoundry();

            //var redisOptions = new RedisCacheConnectorOptions(configBuilder.Build());

            //var xmlDoc = new XmlDocument();

            //xmlDoc.Load(webConfig);

            //var sessionState = xmlDoc.SelectSingleNode("//configuration/system.web/sessionState");

            //if (sessionState != null)
            //    sessionState.RemoveAll();
            //else
            //{
            //    sessionState = xmlDoc.CreateElement("sessionState");
            //    xmlDoc.SelectSingleNode("//configuration/system.web").AppendChild(sessionState);
            //}

            //var modeAttribute = xmlDoc.CreateAttribute("mode");
            //modeAttribute.Value = "Custom";
            //sessionState.Attributes.Append(modeAttribute);

            //var customProviderAttribute = xmlDoc.CreateAttribute("customProvider");
            //customProviderAttribute.Value = "RedisSessionStateStore";
            //sessionState.Attributes.Append(customProviderAttribute);

            //var providersNode = xmlDoc.CreateElement("providers");

            //var providerElement = xmlDoc.CreateElement("add");
            //var providerNameAttribute = xmlDoc.CreateAttribute("name");
            //providerNameAttribute.Value = "RedisSessionStateStore";
            //providerElement.Attributes.Append(providerNameAttribute);

            //var providerTypeAttribute = xmlDoc.CreateAttribute("type");
            //providerTypeAttribute.Value = "Microsoft.Web.Redis.RedisSessionStateProvider";
            //providerElement.Attributes.Append(providerTypeAttribute);

            //var providerConnStringAttribute = xmlDoc.CreateAttribute("connectionString");
            //providerConnStringAttribute.Value = redisOptions.ToString();
            //providerElement.Attributes.Append(providerConnStringAttribute);

            //providersNode.AppendChild(providerElement);
            //sessionState.AppendChild(providersNode);


            ////-------------------
            //var machineKey = xmlDoc.SelectSingleNode("//configuration/system.web/machineKey");

            //if (machineKey != null)
            //    machineKey.RemoveAll();
            //else
            //{
            //    machineKey = xmlDoc.CreateElement("machineKey");
            //    xmlDoc.SelectSingleNode("//configuration/system.web").AppendChild(machineKey);
            //}

            //var validationKeyAttribute = xmlDoc.CreateAttribute("validationKey");
            //validationKeyAttribute.Value = Crypto.KeyCreator.CreateKey(64);
            //machineKey.Attributes.Append(validationKeyAttribute);

            //var decryptionKeyAttribute = xmlDoc.CreateAttribute("decryptionKey");
            //decryptionKeyAttribute.Value = Crypto.KeyCreator.CreateKey(24);
            //machineKey.Attributes.Append(decryptionKeyAttribute);

            //var validationAttribute = xmlDoc.CreateAttribute("validation");
            //validationAttribute.Value = "SHA1";
            //machineKey.Attributes.Append(validationAttribute);

            //xmlDoc.Save(webConfig);
        }
    }
}