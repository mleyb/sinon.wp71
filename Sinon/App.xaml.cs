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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Sinon.Core.Models;
using Sinon.Services;
using Sinon.ViewModels;
using System.Diagnostics;
using BugSense;
using Microsoft.Phone.Notification;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Net.NetworkInformation;
using System.Windows.Threading;
using Microsoft.Phone.Scheduler;

namespace Sinon
{
    public partial class App : Application
    {
        private static MainViewModel _viewModel;

        public static ConfigurationSettingsService ConfigurationSettingsService { get; private set; }

        public static AzureMobileService AzureMobileService { get; private set; }

        public static HttpNotificationChannel PushChannel { get; private set; }

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (_viewModel == null)
                    _viewModel = new MainViewModel();
                
                return _viewModel;
            }
        }

        public static bool IsSignedIn
        {
            get
            {
                return (ConfigurationSettingsService.ConfigSettings["UserId"] != null) &&
                       (ConfigurationSettingsService.ConfigSettings["Token"] != null); 
            }
        }

        public static bool HasDataConnection
        {
            get { return (NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.None); }
        }

        public static void ShowNoDataConnectionMessageBox()
        {
            Dispatcher dispatcher = App.Current.RootVisual.Dispatcher;
            dispatcher.BeginInvoke(() => MessageBox.Show("The request could not be completed. Please try again.", "No Data Connection", MessageBoxButton.OK));
        }

        public static void ShowGeneralErrorMessageBox()
        {
            Dispatcher dispatcher = App.Current.RootVisual.Dispatcher;
            dispatcher.BeginInvoke(() => MessageBox.Show("An error occurred. Please try again.", "Error", MessageBoxButton.OK));
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            BugSenseHandler.Instance.Init(this, "47a9b506", new NotificationOptions { Text = "An error occurred which has been logged. Please try again.", Type = enNotificationType.MessageBox });

            BugSenseHandler.Instance.UnhandledException += Application_UnhandledException; 

            // Standard Silverlight initialization
            InitializeComponent();

            ConfigurationSettingsService = (ConfigurationSettingsService)App.Current.ApplicationLifetimeObjects[1];
            AzureMobileService = (AzureMobileService)App.Current.ApplicationLifetimeObjects[2];

            ConfigurationSettingsService.ConfigurationSettingsLoaded += new EventHandler((sender, args) =>
            {
                // Phone-specific initialization 
                InitializePhoneApplication();

                RootFrame.Navigating += RootFrame_Navigating;

                // Show graphics profiling information while debugging.
                if (Debugger.IsAttached)
                {
                    // Display the current frame rate counters
                    Application.Current.Host.Settings.EnableFrameRateCounter = true;

                    // Show the areas of the app that are being redrawn in each frame.
                    //Application.Current.Host.Settings.EnableRedrawRegions = true;

                    // Enable non-production analysis visualization mode, 
                    // which shows areas of a page that are handed off to GPU with a colored overlay.
                    //Application.Current.Host.Settings.EnableCacheVisualization = true;

                    // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                    // application's PhoneApplicationService object to Disabled.
                    // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                    // and consume battery power when the user is not using the phone.
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
                }
            });
        }        

        public async Task SignIn(MobileServiceAuthenticationProvider provider)
        {
            try
            {
                MobileServiceUser user = await App.AzureMobileService.Client.LoginAsync(provider);

                ConfigurationSettingsService.ConfigSettings["UserId"] = user.UserId;
                ConfigurationSettingsService.ConfigSettings["Token"] = user.MobileServiceAuthenticationToken;
                ConfigurationSettingsService.SaveConfigSettings();

                AzureMobileService.Client.CurrentUser = user;

                await UpdatePushChannelInfo();

                RootFrame.Dispatcher.BeginInvoke(() =>
                {
                    RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                });
            }
            catch (MobileServiceInvalidOperationException)
            {
                MessageBox.Show("You must sign in.");

                RootFrame.Dispatcher.BeginInvoke(() =>
                {
                    RootFrame.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                BugSenseHandler.Instance.LogError(ex);

                ShowGeneralErrorMessageBox();

                RootFrame.Dispatcher.BeginInvoke(() =>
                {
                    RootFrame.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));
                });
            }
        }

        public void SignOut()
        {
            if (AzureMobileService.Client.CurrentUser != null)
            {
                AzureMobileService.Client.Logout();
                AzureMobileService.Client.CurrentUser = null;

                ConfigurationSettingsService.ConfigSettings.Remove("UserId");
                ConfigurationSettingsService.ConfigSettings.Remove("Token");
                ConfigurationSettingsService.SaveConfigSettings();
            }
        }

        private void AcquirePushChannel()
        {
            PushChannel = HttpNotificationChannel.Find("Sinon");

            if (PushChannel == null)
            {
                PushChannel = new HttpNotificationChannel("Sinon");

                PushChannel.ChannelUriUpdated += PushChannel_ChannelUriUpdated;
                PushChannel.ErrorOccurred += PushChannel_ErrorOccurred;
                PushChannel.ShellToastNotificationReceived += PushChannel_ShellToastNotificationReceived;

                PushChannel.Open();

                if (Boolean.Parse(ConfigurationSettingsService.ConfigSettings["NotificationsEnabled"]))
                {
                    PushChannel.BindToShellToast();
                }
            }
            else
            {
                // already open, just bind event handlers
                PushChannel.ChannelUriUpdated += PushChannel_ChannelUriUpdated;
                PushChannel.ErrorOccurred += PushChannel_ErrorOccurred;
                PushChannel.ShellToastNotificationReceived += PushChannel_ShellToastNotificationReceived;
            }
        }

        private async Task UpdatePushChannelInfo(bool deleteOldChannel = false)
        {            
            if (HasDataConnection)
            {
                try
                {
                    if (deleteOldChannel && ConfigurationSettingsService.ConfigSettings["PushChannelUri"] != null)
                    {
                        // delete existing channel info
                        var oldChannel = new PushChannel { Id = Int32.Parse(ConfigurationSettingsService.ConfigSettings["PushChannelId"]), Uri = ConfigurationSettingsService.ConfigSettings["PushChannelUri"] };
                        await App.AzureMobileService.Client.GetTable<PushChannel>().DeleteAsync(oldChannel);
                    }

                    // upload the channel information 
                    var newChannel = new PushChannel { Uri = PushChannel.ChannelUri.ToString() };
                    await App.AzureMobileService.Client.GetTable<PushChannel>().InsertAsync(newChannel);

                    // persist the data locally now we know the channel Id that the server has given us
                    ConfigurationSettingsService.ConfigSettings["PushChannelId"] = newChannel.Id.ToString();
                    ConfigurationSettingsService.ConfigSettings["PushChannelUri"] = newChannel.Uri.ToString();
                    ConfigurationSettingsService.SaveConfigSettings();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());

                    // break into the debugger, or just ignore the exception as the attempt will be retried on 
                    // next activation anyway
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }
        }

        private void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            if (ConfigurationSettingsService.ConfigSettings["PushChannelUri"] != e.ChannelUri.ToString())
            {                
                if (HasDataConnection)
                {                    
                    TaskEx.Run(async () =>
                    {
                        // channel URI has changed, update server with new info
                        await UpdatePushChannelInfo(true);
                    });
                }
            }
        }

        private void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            Debug.WriteLine(String.Format("A push notification {0} error occurred.  {1} ({2}) {3}", e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData));
        }

        private void PushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            StringBuilder message = new StringBuilder();
            string relativeUri = String.Empty;

            message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {
                message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

                if (String.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    relativeUri = e.Collection[key];
                }
            }

            Debug.WriteLine(message);
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            AcquirePushChannel();

            if (IsSignedIn)
            {
                TaskEx.Run(async () =>
                {
                    await UpdatePushChannelInfo();
                });
            }
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately

            TaskEx.Run(async () =>
            {
                await UpdatePushChannelInfo();
            });            
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            ConfigurationSettingsService.SaveConfigSettings();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            ConfigurationSettingsService.SaveConfigSettings();
        }
        
        // Code to intercept MainPage navigation and redirect to SignInPage if the user 
        // is not signed in
        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().Contains("/MainPage.xaml"))
            {
                if (!IsSignedIn)
                {
                    e.Cancel = true;

                    RootFrame.Dispatcher.BeginInvoke(() =>
                    {
                        RootFrame.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));
                    });
                }
                else
                {
                    RootFrame.Dispatcher.BeginInvoke(() =>
                    {
                        RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    });
                }
            }
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, BugSenseUnhandledExceptionEventArgs e)
        {
            ShowGeneralErrorMessageBox();

            BugSenseHandler.Instance.LogError(e.ExceptionObject);

            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }                
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}