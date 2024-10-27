using InstrumentStatusWorker.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InstrumentStatusWorker.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyNotifyIcon = new NotifyIcon();
            MyNotifyIcon.Icon = new System.Drawing.Icon(System.IO.Path.Combine(Environment.CurrentDirectory, "Assets/TaskBarIcon.ico"));
            MyNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MyNotifyIcon_MouseDoubleClick);

            if (Debugger.IsAttached)
            {
                this.ShowInTaskbar = true;
                MyNotifyIcon.Visible = false;
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.ShowInTaskbar = false;
                MyNotifyIcon.Visible = true;
                this.WindowState = WindowState.Minimized;
            }

            DataContext = new MainViewModel();
        }

        NotifyIcon MyNotifyIcon { get; set; }

        void MyNotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                MyNotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult dialog = System.Windows.Forms.MessageBox.Show("Do you want to minimize to the system tray instead of closing?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.None);

            if (dialog == System.Windows.Forms.DialogResult.Yes)
            {
                e.Cancel = true;

                this.WindowState = WindowState.Minimized;
            }
        }
    }
}
