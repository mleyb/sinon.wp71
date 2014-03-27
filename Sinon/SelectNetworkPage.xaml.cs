using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Sinon.ViewModels;

namespace Sinon
{
    public partial class SelectNetworkPage : PhoneApplicationPage
    {
        public SelectNetworkPage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(SelectNetworkPage_Loaded);
        }

        private async void SelectNetworkPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ((SelectNetworkViewModel)Resources["ViewModel"]).LoadData();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.Content is MainPage)
            {
                // we're going back to the main page, so reload all data
                await App.ViewModel.LoadData();
            }

            base.OnNavigatedFrom(e);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NetworkViewModel networkViewModel = ((ListBox)sender).SelectedItem as NetworkViewModel;

            App.ConfigurationSettingsService.ConfigSettings["NetworkId"] = networkViewModel.NetworkId.ToString();

            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}