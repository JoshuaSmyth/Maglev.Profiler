using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MaglevProfiler;
using MaglevProfiler.ClientServer;

namespace TinyFrog.Profiler.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for ConnectionStatus.xaml
    /// </summary>
    public partial class ConnectionStatus : UserControl
    {
        private ProfilerTcpClient m_ProfilerTcpClient;

        private Boolean m_IsConnected;

        public ConnectionStatus()
        {
            InitializeComponent();
            InitializeTcpClient();
        }

        public ProfilerTcpClient ProfilerTcpClient
        {
            get { return m_ProfilerTcpClient; }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectOrDisconnect();
        }

        private void ConnectOrDisconnect()
        {
            if (m_IsConnected)
            {
                ProfilerTcpClient.Disconnect();
                btnConnect.Content = "Connect";
                m_IsConnected = false;
            }
            else
            {
                ProfilerTcpClient.Connect(txtIPAddress.Text, txtPort.Text);
            }
        }

        private void InitializeTcpClient()
        {
            m_ProfilerTcpClient = new ProfilerTcpClient();

            ProfilerTcpClient.OnCommunicationError += (e) =>
            {
                if (e is Win32Exception)
                {
                    var win32Exception = e as Win32Exception;
                    if (win32Exception.ErrorCode == 10061)
                    {
                        MessageBox.Show("No server found");
                        return;
                    }
                }

                if (e.InnerException != null)
                {
                    var innerException = e.InnerException;

                    if (innerException is UnknownMessageException)
                    {
                        MessageBox.Show("Unknown Message recieved from Server");
                        return;
                    }

                    if (innerException is UnableToDecodeMessageException)
                    {
                        MessageBox.Show(String.Format("Unable to decode message :{0}", innerException.Message));
                        return;
                    }
                }
                throw e;
            };

            ProfilerTcpClient.OnConnectedToServer += () => btnConnect.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                                  new Action(() =>
                                                                                      {
                                                                                          m_IsConnected = true;
                                                                                        btnConnect.Content = "Disconnect";
                                                                                        txtStatus.Text = "Connected!";
                                                                                        txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 200, 0));
                                                                                  }
                                                                              ));
        }

        private void TxtIPAddress_OnPopulating(object sender, PopulatingEventArgs e)
        {
            // TODO Load from file
            var items = new List<String>()
                {
                    "localhost",
                    "10.1.1.9",
                    "10.1.1.11"
                };

            txtIPAddress.ItemsSource = items;
            txtIPAddress.PopulateComplete();
        }

        private void TxtIPAddress_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConnectOrDisconnect();
            }
        }

        private void TxtPort_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConnectOrDisconnect();
            }
        }

        private void TxtPort_OnPopulating(object sender, PopulatingEventArgs e)
        {
            // TODO Load from file
            var items = new List<String>()
                {
                    "3141",
                    "3000",
                };

            txtPort.ItemsSource = items;
            txtPort.PopulateComplete();
        }
    }
}
