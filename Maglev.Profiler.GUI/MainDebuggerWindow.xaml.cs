using System;
using System.Windows;

namespace WorldEditor.Debugger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainDebuggerWindow : Window
    {
        public MainDebuggerWindow()
        {
            InitializeComponent();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(this);
            var client = ConnectionStatusControl.ProfilerTcpClient;
            GameProfilerControl.SetTcpClient(client);
            RemoteClassModifierControl.SetTcpClient(client);
            GlobalLogControl.SetTcpClient(client);
        }

        public void SetDefaultPort(Int32 value)
        {
            ConnectionStatusControl.txtPort.Text = value.ToString();
        }
    }
}
