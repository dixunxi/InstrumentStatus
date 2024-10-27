using InstrumentMonitorDashboard.View.Windows;
using InstrumentMonitorDashboard.ViewModels;
using InstrumentStatusService.Controllers;
using InstrumentStatusService.Dbml;
using SafireMvvm.Utilities;
using SafireMvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InstrumentStatusDashboard.ViewModels
{
    public class InstrumentPanelViewModel : BaseViewModel
    {
        public InstrumentPanelViewModel(tbInstrumentStatus_Computer tbTempInstrument) : base("")
        {
            Instrument = tbTempInstrument;

            using (InstrumentStatusController controller = new InstrumentStatusController())
            {
                LastDirectory = controller.GetDirectories(Instrument.id).OrderByDescending(i => i.last_file_modified_date).FirstOrDefault();
                //declared on line 68
            }

            if (LastDirectory != null)
            {
                if (LastDirectory.last_file_modified_date > DateTime.Now.AddMinutes(-15))
                {
                    IsRunning = true;
                }
                else
                {
                    IsRunning = false;
                }
            }
            else
            {
                IsRunning = false;
            }

            if (Instrument.last_ran > DateTime.Now.AddMinutes(-5))
            {
                CurrentStatus = "Worker Online";
                IsOnline = true;
            }
            else
            {
                CurrentStatus = "Worker Offline";
                IsOnline = false;
            }
        }

        public tbInstrumentStatus_Computer Instrument
        {
            get
            {
                return GetValue(() => Instrument);
            }
            set
            {
                SetValue(() => Instrument, value);
            }
        }

        public tbInstrumentStatus_Directory LastDirectory
        {
            get
            {
                return GetValue(() => LastDirectory);
            }
            set
            {
                SetValue(() => LastDirectory, value);
            }
        }

        public string CurrentStatus
        {
            get
            {
                return GetValue(() => CurrentStatus);
            }
            set
            {
                SetValue(() => CurrentStatus, value);
            }
        }

        public bool IsOnline
        {
            get
            {
                return GetValue(() => IsOnline);
            }
            set
            {
                SetValue(() => IsOnline, value);
            }
        }

        public bool IsRunning
        {
            get
            {
                return GetValue(() => IsRunning);
            }
            set
            {
                SetValue(() => IsRunning, value);
            }
        }

        private ICommand _editDirectoriesCommand;

        public ICommand EditDirectoriesCommand
        {
            get
            {
                if (_editDirectoriesCommand == null)
                {
                    _editDirectoriesCommand = new RelayCommand((prop) =>
                    {
                        InstrumentEditWindow instrumentEditWindow = new InstrumentEditWindow(new InstrumentEditViewModel(this));

                        instrumentEditWindow.ShowDialog();
                    });
                }

                return _editDirectoriesCommand;
            }
        }
    }
}
