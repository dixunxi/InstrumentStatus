using SafireMvvm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstrumentMonitorDashboard.Models
{
    public class InstrumentDirectory : NotifyProperty
    {
        private int _id;

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;

                OnPropertyChanged("Id");
            }
        }

        private string _directoryLocation;

        public string DirectoryLocation
        {
            get
            {
                return _directoryLocation;
            }
            set
            {
                _directoryLocation = value;

                OnPropertyChanged("DirectoryLocation");
            }
        }

        private string _fileWildcard;

        public string FileWildcard
        {
            get
            {
                return _fileWildcard;
            }
            set
            {
                _fileWildcard = value;

                OnPropertyChanged("FileWildcard");
            }
        }

        private int _injectionInterval;

        public int InjectionInterval
        {
            get
            {
                return _injectionInterval;
            }
            set
            {
                _injectionInterval = value;

                OnPropertyChanged("InjectionInterval");
            }
        }
    }
}
