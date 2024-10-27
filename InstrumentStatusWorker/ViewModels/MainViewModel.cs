using InstrumentMonitorWorker.Properties;
using InstrumentStatusService.Controllers;
using InstrumentStatusService.Dbml;
using SafireMvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace InstrumentStatusWorker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private static DispatcherTimer _timer;

        public MainViewModel() : base("Instrument Monitor Worker" + " - " + Assembly.GetExecutingAssembly().GetName().Version)
        {
            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(OnTimedEvent);
            _timer.Interval = TimeSpan.FromSeconds(60);
            _timer.Start();

            Log = new ObservableCollection<string>();

            if (Environment.CurrentDirectory.Contains(Settings.Default.KillLocation))
            {
                MessageBox.Show(@"Please copy to your local computers C:\temp folder");

                Environment.Exit(0);
            }

            using (InstrumentStatusController controller = new InstrumentStatusController())
            {
                tbInstrumentStatus_ComputerType tempType = controller.GetComputerType("Worker");

                tbInstrumentStatus_Computer current = controller.GetInstrument(Environment.MachineName, tempType.id);

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
                else
                {
                    current.runtime_location = Environment.CurrentDirectory;
                    current.runtime_version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    current.last_ran = DateTime.Now;
                }

                controller.AddOrUpdateInstrument(current, "Worker");
            }
        }

        public ObservableCollection<string> Log
        {
            get { return GetValue(() => Log); }
            set {  SetValue(() => Log, value); }
        }

        public void RunService()
        {
            using (InstrumentStatusController controller = new InstrumentStatusController())
            {
                tbInstrumentStatus_ComputerType tempType = controller.GetComputerType("Worker");

                tbInstrumentStatus_Computer tempComputer = controller.GetInstrument(Environment.MachineName, tempType.id);

                controller.UpdateLastRan(Environment.MachineName, tempType.id);

                if (tempComputer.is_active)
                {
                    List<tbInstrumentStatus_Directory> tbTempDirectories = new List<tbInstrumentStatus_Directory>(controller.GetDirectories(tempComputer.id));

                    foreach (var directory in tbTempDirectories)
                    {
                        IEnumerable<FileInfo> files = null;

                        //DirectoryInfo directoryInfo = Directory.GetDirectories(directory.directory_location).Select(i => new DirectoryInfo(i)).OrderByDescending(i => i.LastWriteTime).FirstOrDefault();

                        IEnumerable<DirectoryInfo> directories = Directory.GetDirectories(directory.directory_location).Select(i => new DirectoryInfo(i)).OrderByDescending(i => i.LastAccessTime);

                        // you can recursion call this but I didn't have time to do so
                        if (directories.Count() > 0)
                        {
                            foreach (var item in directories)
                            {
                                //MessageBox.Show(directories.Count().ToString());
                                IEnumerable<DirectoryInfo> subDirectories = Directory.GetDirectories(item.FullName).Select(i => new DirectoryInfo(i)).OrderByDescending(i => i.LastAccessTime);
                                //MessageBox.Show(subDirectories.Count().ToString());
                                if (subDirectories.Count() > 0)
                                {
                                    WriteToLog("Searching second level subdirectories...");
                                    //MessageBox.Show(subDirectories.First().FullName);
                                    //string targetDirectory = Directory.GetDirectories(subDirectories.First().FullName).Select(i => new DirectoryInfo(i)).OrderByDescending(i => i.LastAccessTime).First().FullName;

                                    files = Directory.GetFiles(subDirectories.First().FullName, directory.file_wildcard, SearchOption.AllDirectories).Select(i => new FileInfo(i));
                                    //MessageBox.Show(files.Count().ToString());

                                    WriteToLog($"Searching '{subDirectories.First().FullName}' for files matching {directory.file_wildcard}");

                                    break;
                                }
                                else
                                {
                                    WriteToLog("Searching subdirectories...");

                                    files = Directory.GetFiles(item.FullName, directory.file_wildcard).Select(i => new FileInfo(i));

                                    WriteToLog($"Searching '{item.FullName}' for files matching {directory.file_wildcard}");

                                    break;
                                }
                            }
                        }
                        else
                        {
                            files = Directory.GetFiles(directory.directory_location, directory.file_wildcard).Select(i => new FileInfo(i));

                            WriteToLog($"Searching '{directory.directory_location}' for files matching {directory.file_wildcard}");
                        }

                        if (files != null)
                        {
                           // MessageBox.Show(files.Count().ToString());
                            foreach (var file in files.OrderBy(i => i.LastWriteTime))
                            {
                                if (file.LastWriteTime > DateTime.Now.AddMinutes(directory.injection_interval * (-1)))
                                {
                                    WriteToLog($"Found '{file.Name}' with last write time of '{file.LastWriteTime}'");

                                    try
                                    {
                                        controller.FileModified(directory.id, file);
                                    }
                                    catch (Exception e)
                                    {
                                        WriteToLog("Error writing to database, " + e.Message);
                                    }

                                    break;
                                }
                            }
                        }
                        else
                        {
                            WriteToLog($"Failed to find any files or directories in {directory}");
                        }
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        public void WriteToLog(string log)
        {
            if (Log.Count() > 100)
            {
                Log.Remove(Log.Last());
            }

            Log.Insert(0, log);
        }

        private void OnTimedEvent(object source, EventArgs e)
        {
            RunService();
        }
    }
}
