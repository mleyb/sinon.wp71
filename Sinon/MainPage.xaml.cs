using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Sinon.Core.Models;
using Microsoft.Phone.Shell;
using System.Windows.Navigation;
using Sinon.ViewModels;
using Coding4Fun.Toolkit.Controls;
using System.Reflection;

namespace Sinon
{
    public partial class MainPage : PhoneApplicationPage
    {               
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // remove the Sign In page from the back stack
            NavigationService.RemoveBackEntry();

            base.OnNavigatedTo(e);
        }

        // Load data for the ViewModel Items
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {                
                await App.ViewModel.LoadData();
            }
        }

        private async void OnRefreshButtonClick(object sender, EventArgs e)
        {
            await App.ViewModel.RefreshSightings();
        }

        private void OnNetworkButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SelectNetworkPage.xaml", UriKind.Relative));
        }

        private void OnSettingsButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void OnAboutItemClick(object sender, EventArgs e)
        {
            String appVersion = "v" + Assembly.GetExecutingAssembly().GetCustomAttributes(false).OfType<AssemblyFileVersionAttribute>().First().Version;

            AboutPrompt about = new AboutPrompt();
            about.Title = "Trammy Dodger";
            about.VersionNumber = appVersion;

            AboutPromptItem item = new AboutPromptItem();
            item.AuthorName = "Blue Zero";   
            item.WebSiteUrl = @"www.bluezero.co.uk";

            about.Show(item);
        }

        private void OnSignOutItemClick(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure?", "Sign Out", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                ((App)App.Current).SignOut();             
            }
        }

        private async void StationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StationViewModel stationViewModel = ((LongListSelector)sender).SelectedItem as StationViewModel;

            await stationViewModel.ReportNewSighting();
            
            // move to first pivot page
            MainPivot.SelectedIndex = 0;

            await App.ViewModel.RefreshSightings();
        }
    }
}