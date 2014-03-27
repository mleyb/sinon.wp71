using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
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
    public class SightingViewModel : ViewModelBase
    {
        private string _location;

        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (value != _location)
                {
                    _location = value;
                    NotifyPropertyChanged("Location");
                }
            }
        }

        private string _timestamp;

        public string Timestamp
        {
            get
            {
                return _timestamp;
            }
            set
            {
                if (value != _timestamp)
                {
                    _timestamp = value;
                    NotifyPropertyChanged("Timestamp");
                }
            }
        }
    }
}