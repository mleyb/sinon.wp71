using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Sinon.Services
{
    public class ConfigurationSettingsService : IApplicationService
    {
        public event EventHandler ConfigurationSettingsLoaded;

        public Dictionary<string, string> ConfigSettings = new Dictionary<string, string>();

        public ConfigurationSettingsService()
        {
            // set defaults
            ConfigSettings.Add("UserId", null);
            ConfigSettings.Add("Token", null);            
            ConfigSettings.Add("PushChannelId", (-1).ToString());
            ConfigSettings.Add("PushChannelUri", null);
            ConfigSettings.Add("NetworkId", 1.ToString());
            ConfigSettings.Add("NetworkName", null);
            ConfigSettings.Add("NotificationsEnabled", Boolean.TrueString);
        }

        #region IApplicationService Members

        void IApplicationService.StartService(ApplicationServiceContext context)
        {
            LoadConfigSettings();

            if (ConfigurationSettingsLoaded != null)
                ConfigurationSettingsLoaded(this, EventArgs.Empty); // signal ready
        }

        void IApplicationService.StopService()
        {
            SaveConfigSettings();
        }

        #endregion        

        public void SaveConfigSettings()
        {
            Debug.WriteLine("Saving configuration settings");

            if (ConfigSettings["UserId"] != null)
            {
                IsolatedStorageSettings.ApplicationSettings["UserId"] = ConfigSettings["UserId"];
            }

            if (ConfigSettings["Token"] != null)
            {
                byte[] encryptedTokenBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(ConfigSettings["Token"]), null);
                IsolatedStorageSettings.ApplicationSettings["Token"] = encryptedTokenBytes;
            }

            if (Int32.Parse(ConfigSettings["PushChannelId"]) != -1)
            {
                IsolatedStorageSettings.ApplicationSettings["PushChannelId"] = ConfigSettings["PushChannelId"];
            }

            if (ConfigSettings["PushChannelUri"] != null)
            {
                IsolatedStorageSettings.ApplicationSettings["PushChannelUri"] = ConfigSettings["PushChannelUri"];
            }

            IsolatedStorageSettings.ApplicationSettings["NetworkId"] = ConfigSettings["NetworkId"];

            IsolatedStorageSettings.ApplicationSettings["NotificationsEnabled"] = ConfigSettings["NotificationsEnabled"];

            IsolatedStorageSettings.ApplicationSettings.Save();
        }        

        private void LoadConfigSettings()
        {
            Debug.WriteLine("Loading configuration settings");

            if (IsolatedStorageSettings.ApplicationSettings.Contains("UserId"))
            {
                ConfigSettings["UserId"] = (String)IsolatedStorageSettings.ApplicationSettings["UserId"];
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("Token"))
            {
                byte[] encryptedTokenBytes = (byte[])IsolatedStorageSettings.ApplicationSettings["Token"];
                byte[] unencryptedTokenBytes = ProtectedData.Unprotect(encryptedTokenBytes, null);
                ConfigSettings["Token"] = Encoding.UTF8.GetString(unencryptedTokenBytes, 0, unencryptedTokenBytes.Length);
            }            

            if (IsolatedStorageSettings.ApplicationSettings.Contains("PushChannelId"))
            {
                ConfigSettings["PushChannelId"] = (String)IsolatedStorageSettings.ApplicationSettings["PushChannelId"];
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("PushChannelUri"))
            {
                ConfigSettings["PushChannelUri"] = (String)IsolatedStorageSettings.ApplicationSettings["PushChannelUri"];
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("NetworkId"))
            {
                ConfigSettings["NetworkId"] = (String)IsolatedStorageSettings.ApplicationSettings["NetworkId"];
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("NotificationsEnabled"))
            {
                ConfigSettings["NotificationsEnabled"] = (String)IsolatedStorageSettings.ApplicationSettings["NotificationsEnabled"];
            }
        }        
    }
}
