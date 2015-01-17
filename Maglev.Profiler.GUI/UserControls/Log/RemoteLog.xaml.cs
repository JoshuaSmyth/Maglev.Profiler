using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MaglevProfiler.ClientServer;

namespace TinyFrog.Profiler.Gui.UserControls.Log
{
    /// <summary>
    /// Interaction logic for RemoteLog.xaml
    /// </summary>
    public partial class RemoteLog : UserControl
    {
        private ProfilerTcpClient m_Client;

        public RemoteLog()
        {
            InitializeComponent();
        }


        public void SetTcpClient(ProfilerTcpClient client)
        {
            m_Client = client;
            client.OnRecievedGlobalLogData += (d) => btnGetLog.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                                          new Action(() =>
                                                                                              {txtLog.Text = d; }));
        }

     
        private void BtnGetLog_OnClick(object sender, RoutedEventArgs e)
        {
            m_Client.SendRequestGlobalLogMessage();
        }
    }
}
