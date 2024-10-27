using InstrumentMonitorDashboard.Properties;
using InstrumentStatusService.Controllers;
using InstrumentStatusService.Dbml;
using SafireMvvm.Utilities;
using SafireMvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace InstrumentStatusDashboard.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private static Timer _oneMinuteTimer = new Timer(1 * 60 * 1000);
        // why is it written like 1 * 60 * 1000  1000 is for milliseconds times 60 for every minute
        //thirds hours

        public MainWindowViewModel() : base("Instrument Monitor Dashboard")
        {
            _oneMinuteTimer.Elapsed += new ElapsedEventHandler(OnOneMinuteTimedEvent);
            // every time the timer lapses elapsed event handler refreshes the dashboard instruments list
            _oneMinuteTimer.Enabled = true;

            if (Environment.CurrentDirectory.Contains(Settings.Default.KillLocation))
            {
                MessageBox.Show(@"Please copy to your local computers C:\temp folder");

                Environment.Exit(0);
            }

            using (InstrumentStatusController controller = new InstrumentStatusController())
            {
                tbInstrumentStatus_ComputerType tempType = controller.GetComputerType("Dashboard");

                tbInstrumentStatus_Computer current = controller.GetInstrument(Environment.MachineName, tempType.id);
                // above checks to see if this computer under the type dashboard has logged on before. If not directly below
                // it prepares a new current to be submitted that will create a record for this computer.

                if (current == null)
                {
                    current = new tbInstrumentStatus_Computer()
                    {
                        id = -1,
                        system_name = Environment.MachineName,
                        runtime_location = Environment.CurrentDirectory,
                        runtime_version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                        is_active = true,
                        last_ran = DateTime.Now
                    };
                }
                // if a record already exists a new one is not created and below the location version and
                // last ran date are updated
                else
                {
                    current.runtime_location = Environment.CurrentDirectory;
                    current.runtime_version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    current.last_ran = DateTime.Now;
                }

                controller.AddOrUpdateInstrument(current, "Dashboard");
            }

            OnStart();
        }

        public ObservableCollection<InstrumentPanelViewModel> InstrumentPanelList
        {
            get
            {
                return GetValue(() => InstrumentPanelList);
            }
            set
            {
                SetValue(() => InstrumentPanelList, value);
            }
        }

        public IEnumerable<tbInstrumentStatus_Computer> InstrumentData { get; set; }

        private async Task<List<tbInstrumentStatus_Computer>> LoadInstrumentData()
        {
            try
            {
                using (InstrumentStatusController controller = new InstrumentStatusController())
                {
                    tbInstrumentStatus_ComputerType tempType = controller.GetComputerType("Worker");
                    //above calls queries database for Worker id wich is 1 to get all
                    //computers(instruments) for worker and order by the system name

                    return await Task.Run(() => controller.GetInstruments(tempType.id).OrderBy(i => i.system_name).ToList()).ConfigureAwait(false);
                }
            }
            catch
            {
                return LoadInstrumentData().Result; 
                // looks like if this fails it returns the failed results of the call.
            }
        }

        private void LoadInstrumentPanels()
        {
            //Property for Instrument PanelList below is declared on line 72 and this info is
            //bound in wpf to list out each instrument on the main page 
            InstrumentPanelList = new ObservableCollection<InstrumentPanelViewModel>(InstrumentData.Select(i => new InstrumentPanelViewModel(i)));
            // i believe the way the i variable is used above it grabs all of them in the list.
        }

        private void OnOneMinuteTimedEvent(object source, ElapsedEventArgs e)
        {
            InstrumentData = LoadInstrumentData().Result;

            using (InstrumentStatusController controller = new InstrumentStatusController())
            {
                tbInstrumentStatus_ComputerType tempType = controller.GetComputerType("Dashboard");

                tbInstrumentStatus_Computer current = controller.GetInstrument(Environment.MachineName, tempType.id);

                if (!current.is_active)
                {
                    Environment.Exit(0);
                }
            }

            LoadInstrumentPanels();
        }

        private void OnStart()
        {
            InstrumentData = LoadInstrumentData().Result;
            //Property for Instrument PanelList below is declared on line 84 and is used in line 110 to
            // to be sent as an InstrumentStatus_Computer type parameter to find the last_file_modified_date
            //of the directory and if the instrument is online to indicate the background color of the user control
            //offline is salmon online and running is green and online but not running is light yellow.

            LoadInstrumentPanels();
        }

        private ICommand _refreshCommand;

        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand((p) =>
                    {
                        InstrumentData = LoadInstrumentData().Result;

                        LoadInstrumentPanels();
                    });
                }

                return _refreshCommand;
            }
        }

        //The purpose of using the ICommand interface like shown above is it allows the button call within the
        //main page to be listed above within the view model instead of in the mainwindow.cs page for loose coupling
        // so that it can be used in web or mobile applications and not just windows applications. used in mainwindow
        // line 17
        
    }
}
