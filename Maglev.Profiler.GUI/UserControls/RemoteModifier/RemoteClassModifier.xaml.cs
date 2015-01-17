using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MaglevProfiler;
using MaglevProfiler.ClientServer;

namespace TinyFrog.Profiler.Gui.UserControls.RemoteModifier
{
    /// <summary>
    /// Interaction logic for RemoteClassModifier.xaml
    /// </summary>
    public partial class RemoteClassModifier : UserControl
    {
        private ProfilerTcpClient m_Client;
        private PropertyModifierStackPanelBuilder m_StackPanelBuilder = new PropertyModifierStackPanelBuilder();

        public RemoteClassModifier()
        {
            InitializeComponent();
        }

        public void SetTcpClient(ProfilerTcpClient client)
        {
            m_Client = client;
            m_Client.OnRecievedRemoteClassMetaData += (m) => lbClasses.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                                          new Action(() =>
                                                                                                         {
                                                                                                             lbClasses.ItemsSource = m;
                                                                                                             if (m.Any())
                                                                                                             {
                                                                                                                 lbClasses.SelectedItem = m.First();
                                                                                                             }
                                                                                                         }
                                                                             ));
        }

        private void GetData_Click(object sender, RoutedEventArgs e)
        {
            if (m_Client.IsConnected)
            {
                m_Client.SendRequestRemoteClassMetaDataMessage();
            }
            else
            {
                MessageBox.Show("Not connected");
            }
        }

        private void lbClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            propertiesControl.Children.Clear();

            if (lbClasses.SelectedItem == null)
                return;

            var selectedItem = lbClasses.SelectedItem as RemotelyModifiableClassInfo;

            foreach (var property in selectedItem.ClassProperties)
            {
                if (!property.IsStatic)
                {
                    var stackPanel = m_StackPanelBuilder.BuildNotStaticStackPanel(property, SendModifyPropertyMessage);
                    propertiesControl.Children.Add(stackPanel);
                }
                else
                {
                    if (property.IsEnum)
                    {
                        var stackPanel = m_StackPanelBuilder.BuildEnumStackPanel(property, SendModifyPropertyMessage);
                        propertiesControl.Children.Add(stackPanel);
                    }
                    else
                    {
                        if (property.PropertyType == "System.Boolean")
                            propertiesControl.Children.Add(m_StackPanelBuilder.BuildBooleanStackPanel(property, SendModifyPropertyMessage));
   
                        if (property.PropertyType == "System.Int32")
                            propertiesControl.Children.Add(m_StackPanelBuilder.BuildIntegerStackPanel(property, SendModifyPropertyMessage));
            
                        if (property.PropertyType == "System.Single" || property.PropertyType == "System.Double")
                            propertiesControl.Children.Add(m_StackPanelBuilder.BuildSliderStackPanel(property, SendModifyPropertyMessage));
                    }
                }
            }
        }

        private void SendModifyPropertyMessage(RemotelyModifiablePropertyInfo property)
        {
            var currentClass = lbClasses.SelectedItem as RemotelyModifiableClassInfo;
            if (currentClass == null)
                return;

            var msg = new RemotelyModifiableClassInfo(currentClass.ClassName) {AssemblyName = currentClass.AssemblyName};
            msg.ClassProperties.Add(property);

            m_Client.SendModifyPropertyMessage(msg);
        }
    }
}
