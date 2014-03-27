using BugSense;
using Microsoft.WindowsAzure.MobileServices;
using Sinon.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinon.ViewModels
{
    public class SelectNetworkViewModel : ViewModelBase
    {
        public ObservableCollection<NetworkViewModel> Networks { get; private set; }

        private IMobileServiceTable<Network> _networkTable = App.AzureMobileService.Client.GetTable<Network>();

        public SelectNetworkViewModel()
        {
            Networks = new ObservableCollection<NetworkViewModel>();
        }

        public async Task LoadData()
        {
            if (!App.HasDataConnection)
            {
                App.ShowNoDataConnectionMessageBox();
                return;
            }

            IsLoading = true;

            try
            {
                List<Network> networks = await _networkTable.ToListAsync();

                Networks.Clear();

                networks.ForEach(n => Networks.Add(new NetworkViewModel { NetworkId = n.Id, NetworkName = n.Name }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                BugSenseHandler.Instance.LogError(ex);

                App.ShowGeneralErrorMessageBox();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
