using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Sinon.Core.Models;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using BugSense;

namespace Sinon.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IMobileServiceTable<Network> _networkTable = App.AzureMobileService.Client.GetTable<Network>();
        private IMobileServiceTable<Sighting> _sightingTable = App.AzureMobileService.Client.GetTable<Sighting>();
        private IMobileServiceTable<Station> _stationTable = App.AzureMobileService.Client.GetTable<Station>();

        private string _networkName;

        public string NetworkName
        {
            get
            {
                return _networkName;
            }
            set
            {
                if (value != _networkName)
                {
                    _networkName = value;
                    NotifyPropertyChanged("NetworkName");
                }
            }
        }

        public ObservableCollection<SightingViewModel> Sightings { get; private set; }
        public ObservableCollection<Group<StationViewModel>> Stations { get; private set; }
          
        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public MainViewModel()
        {
            NetworkName = "Welcome";
            Sightings = new ObservableCollection<SightingViewModel>();
            Stations = new ObservableCollection<Group<StationViewModel>>();
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
                int selectedNetworkId = GetSelectedNetworkId();

                var getNetworkTask = GetNetwork(selectedNetworkId);
                var getSightingsTask = GetSightings(selectedNetworkId);
                var getStationsTask = GetStations(selectedNetworkId);

                await TaskEx.WhenAll(getNetworkTask, getSightingsTask, getStationsTask);

                NetworkName = getNetworkTask.Result.Name.ToLower();

                Sightings.Clear();

                getSightingsTask.Result.ForEach(s => Sightings.Add(new SightingViewModel { Location = s.StationName, Timestamp = DateTime.Parse(s.DateTime).ToLocalTime().ToShortTimeString() }));

                Stations.Clear();

                // group stations by name, alphabetically
                var query = from station in getStationsTask.Result
                            group station by station.Name[0] into stationGroup
                            orderby stationGroup.Key
                            select stationGroup;

                foreach (var group in query)
                {
                    Group<StationViewModel> stationGroup = new Group<StationViewModel>(group.Key.ToString(), group.Select(s => new StationViewModel { StationId = s.Id, StationName = s.Name }));

                    Stations.Add(stationGroup);
                }

                IsDataLoaded = true;
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

        public async Task RefreshSightings()
        {
            if (!App.HasDataConnection)
            {
                App.ShowNoDataConnectionMessageBox();
                return;
            }

            IsLoading = true;

            try
            {
                int selectedNetworkId = GetSelectedNetworkId();

                List<Sighting> sightings = await GetSightings(selectedNetworkId);

                Sightings.Clear();

                sightings.ForEach(s => Sightings.Add(new SightingViewModel { Location = s.StationName, Timestamp = DateTime.Parse(s.DateTime).ToLocalTime().ToShortTimeString() }));
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

        private async Task<Network> GetNetwork(int networkId)
        {
            IMobileServiceTableQuery<Network> query = _networkTable.Where(x => x.Id == networkId);

            List<Network> results = await query.ToListAsync();

            return results[0];
        }

        private async Task<List<Sighting>> GetSightings(int networkId)
        {
            IMobileServiceTableQuery<Sighting> query = _sightingTable.Where(x => x.NetworkId == networkId).OrderByDescending(x => x.DateTime).Take(200);

            return await query.ToListAsync();            
        }

        private async Task<List<Station>> GetStations(int networkId)
        {
            IMobileServiceTableQuery<Station> query = _stationTable.Where(x => x.NetworkId == networkId).Take(200).OrderBy(x => x.Name);

            return await query.ToListAsync();
        }

        private int GetSelectedNetworkId()
        {
            int networkId = 1; // sensible default
            string setting;

            if (App.ConfigurationSettingsService.ConfigSettings.TryGetValue("NetworkId", out setting))
            {
                networkId = Int32.Parse(setting);
            }

            return networkId;
        }
    }
}