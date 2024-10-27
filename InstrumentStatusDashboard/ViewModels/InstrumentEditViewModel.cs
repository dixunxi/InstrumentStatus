using InstrumentMonitorDashboard.Models;
using InstrumentStatusDashboard.ViewModels;
using InstrumentStatusService.Controllers;
using InstrumentStatusService.Dbml;
using SafireMvvm.Utilities;
using SafireMvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InstrumentMonitorDashboard.ViewModels
{
    public class InstrumentEditViewModel : BaseViewModel
    {
        //Constructor below inherits base class with an input parameter that designates the title by passing in instrumentPanelViewModel.Instrument.system_name
        public InstrumentEditViewModel(InstrumentPanelViewModel instrumentPanelViewModel) : base($"Edit {instrumentPanelViewModel.Instrument.system_name}")
        {
            InstrumentPanelViewModel = instrumentPanelViewModel;    // sets the prop on line 30

            using (InstrumentStatusController controller = new InstrumentStatusController()) // using the database controller to get directory specific info based on the id
            {
                Directories = new ObservableCollection<InstrumentDirectory>(controller.GetDirectories(instrumentPanelViewModel.Instrument.id).Select(i => new InstrumentDirectory() { Id = i.id, DirectoryLocation = i.directory_location, FileWildcard = i.file_wildcard, InjectionInterval = i.injection_interval }));
            }
        }

        public InstrumentPanelViewModel InstrumentPanelViewModel { get; set; }

        public ObservableCollection<InstrumentDirectory> Directories
        {
            get
            {
                return GetValue(() => Directories);
            }
            set
            {
                SetValue(() => Directories, value);
            }
        }

        private ICommand _deleteCommand;

        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand((prop) =>
                    {
                        Directories.Remove(Directories.Where(i => i.Id == Convert.ToInt32(prop)).FirstOrDefault());

                        using (InstrumentStatusController controller = new InstrumentStatusController())
                        {
                            controller.RemoveDirectory(Convert.ToInt32(prop));
                        }
                    });
                }

                return _deleteCommand;
            }
        }

        private ICommand _addCommand;

        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RelayCommand((prop) =>
                    {
                        Directories.Add(new InstrumentDirectory() { Id = Directories.Count() * -1 });
                    });
                }

                return _addCommand;
            }
        }

        private ICommand _saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand((prop) =>
                    {
                        using (InstrumentStatusController controller = new InstrumentStatusController())
                        {
                            foreach (var directory in Directories)
                            {
                                tbInstrumentStatus_Directory temp = new tbInstrumentStatus_Directory();

                                if (directory.Id <= 0)
                                {
                                    temp.injection_interval = directory.InjectionInterval;
                                    temp.instrument_id = InstrumentPanelViewModel.Instrument.id; //gets new id
                                    temp.directory_location = directory.DirectoryLocation;
                                    temp.file_wildcard = directory.FileWildcard;
                                }
                                else
                                {
                                    temp = controller.GetDirectory(directory.Id);   //uses existing id

                                    temp.injection_interval = directory.InjectionInterval;
                                    temp.directory_location = directory.DirectoryLocation;
                                    temp.file_wildcard = directory.FileWildcard;
                                }

                                controller.AddOrUpdateDirectory(temp);
                            }
                        }
                    });
                }

                return _saveCommand;
            }
        }
    }
}
