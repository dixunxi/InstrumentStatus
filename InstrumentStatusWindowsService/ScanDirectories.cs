using InstrumentStatusService.Controllers;
using InstrumentStatusService.Dbml;
using InstrumentStatusWindowsService.Properties;
using LoggingLibrary.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace InstrumentStatusWindowsService
{
    public partial class ScanDirectories : ServiceBase
    {
        private static Timer _timer;

        public ScanDirectories()
        {
            InitializeComponent();

            if (!File.Exists(Settings.Default.LocalLogFile))
            {
                File.Create(Settings.Default.LocalLogFile).Close();
            }

            _timer = new Timer(1 * 60 * 1000);
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.Enabled = true;
        }

        public void RunService()
        {
            using (InstrumentStatusController controller = new InstrumentStatusController())
            {
                List<tbInstrumentStatus_Directory> tbTempDirectories = new List<tbInstrumentStatus_Directory>(controller.GetDirectories(controller.GetInstrument(Environment.MachineName).id));

                string log = $"{Environment.MachineName}" + Environment.NewLine;

                foreach (var directory in tbTempDirectories)
                {
                    log += $"Searching '{directory.directory_location}' for files matching {directory.file_wildcard}" + Environment.NewLine;

                    IEnumerable<FileInfo> files = Directory.GetFiles(directory.directory_location, directory.file_wildcard).Select(i => new FileInfo(i));

                    if (directory.search_sub_directories)
                    {
                        if (Directory.GetDirectories(directory.directory_location).Count() > 0)
                        {
                            log += "Searching subdirectories..." + Environment.NewLine;

                            string targetDirectory = Directory.GetDirectories(directory.directory_location).Select(i => new DirectoryInfo(i)).OrderByDescending(i => i.LastWriteTime).First().FullName;

                            files = Directory.GetFiles(targetDirectory, directory.file_wildcard).Select(i => new FileInfo(i));

                            log += $"Searching '{targetDirectory}' for files matching {directory.file_wildcard}" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        files = Directory.GetFiles(directory.directory_location, directory.file_wildcard).Select(i => new FileInfo(i));

                        log += $"Searching '{directory.directory_location}' for files matching {directory.file_wildcard}" + Environment.NewLine;
                    }

                    foreach (var file in files.OrderBy(i => i.LastWriteTime))
                    {
                        if (file.LastWriteTime > DateTime.Now.AddMinutes(directory.injection_interval * (-1)))
                        {
                            log += $"Found '{file.Name}' with last write time of '{file.LastWriteTime}'" + Environment.NewLine;

                            try
                            {
                                controller.FileModified(directory.id, file);
                            }
                            catch (Exception e)
                            {
                                log += "Error writing to database, " + e.Message;
                            }

                            break;
                        }
                    }
                }

                WriteToEventLog.WriteEvent(log, "DirectoryScan", "DirectoryScanLog");

                if (Settings.Default.WriteToLocalLog)
                {
                    File.WriteAllText(Settings.Default.LocalLogFile, log);
                }
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            RunService();
        }

        protected override void OnStart(string[] args)
        {
            RunService();
        }

        protected override void OnStop()
        {
        }
    }
}
