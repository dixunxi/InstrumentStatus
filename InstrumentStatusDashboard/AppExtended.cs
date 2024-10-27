using InstrumentMonitorDashboard.Properties;
using SafireMvvm.Utilities;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace InstrumentStatusDashboard
{
    /// <summary>
    /// Class to allow for simple overriding of global settings
    /// </summary>
    public class AppExtended
    {
        /// <summary>
        /// Simple constructor. Overrides the default exception settings.
        /// </summary>
        public AppExtended()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalUncaughtException);
        }

        static void GlobalUncaughtException(object sender, UnhandledExceptionEventArgs args)
        {
            if (!Debugger.IsAttached)
            {
                try
                {
                    Exception ex = (Exception)args.ExceptionObject;

                    System.Windows.Forms.MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + "Log file written to " + Settings.Default.LocalErrorLog + ". The plugin will now terminate!", "Converter Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    LoggingLibrary.WriteToLocal.WriteToLogLocal(ex);

                    string safireLog = SafireLog.This.ReadFromLog();

                    if (safireLog.Length > 0)
                    {
                        LoggingLibrary.WriteToLocal.WriteToLogLocal(safireLog, '\n');
                    }
                }
                catch
                {
                    Environment.Exit(0);
                }
            }
        }
    }
}
