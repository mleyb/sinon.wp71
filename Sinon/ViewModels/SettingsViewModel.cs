using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinon.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public bool NotificationsEnabled
        {
            get
            {
                return Boolean.Parse(App.ConfigurationSettingsService.ConfigSettings["NotificationsEnabled"]);
            }
            set
            {
                bool notificationsEnabled = Boolean.Parse(App.ConfigurationSettingsService.ConfigSettings["NotificationsEnabled"]);

                if (value != notificationsEnabled)
                {
                    App.ConfigurationSettingsService.ConfigSettings["NotificationsEnabled"] = value.ToString();
                    App.ConfigurationSettingsService.SaveConfigSettings();

                    SetChannelBindToShellToast(value);

                    NotifyPropertyChanged("NotificationsEnabled");
                }
            }
        }

        private void SetChannelBindToShellToast(bool notificationsEnabled)
        {
            if (notificationsEnabled)
            {
                if (!App.PushChannel.IsShellToastBound)
                {
                    App.PushChannel.BindToShellToast();
                }
            }
            else
            {
                if (App.PushChannel.IsShellToastBound)
                {
                    App.PushChannel.UnbindToShellToast();
                }
            }
        }
    }
}
