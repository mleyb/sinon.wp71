using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sinon.Services
{
    public class AzureMobileService : IApplicationService
    {
        public MobileServiceClient Client { get; private set; }

        public void StartService(ApplicationServiceContext context)
        {
            Client = new MobileServiceClient();

            if (App.IsSignedIn)
            {
                MobileServiceUser user = new MobileServiceUser(App.ConfigurationSettingsService.ConfigSettings["UserId"]);
                user.MobileServiceAuthenticationToken = App.ConfigurationSettingsService.ConfigSettings["Token"];

                Client.CurrentUser = user;
            }
        }

        public void StopService()
        {
            Client.Dispose();
        }
    }
}
