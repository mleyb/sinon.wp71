using BugSense;
using Sinon.Core.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Sinon.ViewModels
{
    public class StationViewModel : ViewModelBase
    {
        private int _stationId;
        private string _stationName;

        public int StationId
        {
            get
            {
                return _stationId;
            }
            set
            {
                if (value != _stationId)
                {
                    _stationId = value;
                    NotifyPropertyChanged("StationId");
                }
            }
        }        

        public string StationName
        {
            get
            {
                return _stationName;
            }
            set
            {
                if (value != _stationName)
                {
                    _stationName = value;
                    NotifyPropertyChanged("StationName");
                }
            }
        }

        public async Task ReportNewSighting()
        {
            if (!App.HasDataConnection)
            {
                App.ShowNoDataConnectionMessageBox();
                return;
            }

            Sighting sighting = new Sighting();
            sighting.NetworkId = Int32.Parse(App.ConfigurationSettingsService.ConfigSettings["NetworkId"]);
            sighting.StationId = StationId;
            sighting.StationName = StationName;
            sighting.DateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mmZ");
            sighting.PushChannelId = Int32.Parse(App.ConfigurationSettingsService.ConfigSettings["PushChannelId"]);

            try
            {
                await App.AzureMobileService.Client.GetTable<Sighting>().InsertAsync(sighting);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                BugSenseHandler.Instance.LogError(ex);

                App.ShowGeneralErrorMessageBox();
            }
        }
    }
}