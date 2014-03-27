using BugSense;
using Microsoft.WindowsAzure.MobileServices;
using Sinon.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Sinon.ViewModels
{
    public class SignInViewModel : ViewModelBase
    {
        private ICommand _signInCommand;

        public ICommand SignInCommand
        {
            get { return _signInCommand; }
        }

        public SignInViewModel()
        {
            _signInCommand = new DelegateCommand(SignIn);
        }

        private async void SignIn(object parameter)
        {
            if (!App.HasDataConnection)
            {
                App.ShowNoDataConnectionMessageBox();
                return;
            }

            MobileServiceAuthenticationProvider provider = (MobileServiceAuthenticationProvider)Enum.Parse(typeof(MobileServiceAuthenticationProvider), parameter.ToString(), true);
                
            await ((App)App.Current).SignIn(provider);
        }
    }
}
