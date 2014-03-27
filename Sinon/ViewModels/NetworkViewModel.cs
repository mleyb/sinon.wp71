using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sinon.ViewModels
{
    public class NetworkViewModel : ViewModelBase
    {
        private int _networkId;

        public int NetworkId
        {
            get
            {
                return _networkId;
            }
            set
            {
                if (value != _networkId)
                {
                    _networkId = value;
                    NotifyPropertyChanged("NetworId");
                }
            }
        }

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
    }
}
