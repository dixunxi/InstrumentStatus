using InstrumentStatusWorker.View.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InstrumentStatusWorker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartUp(object sender, StartupEventArgs e)
        {
            new AppExtended();

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
