using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MaglevProfiler;
using MaglevProfiler.ClientServer;

namespace TinyFrog.Profiler.Gui.UserControls.Profile
{
    /// <summary>
    /// Interaction logic for Profiler.xaml
    /// </summary>
    public partial class GameProfiler : UserControl
    {
        private readonly FrameHistoryChart m_FrameHistoryChart;
        private ProfilerTcpClient m_Client;

        private bool m_IsCollecting;

        public GameProfiler()
        {
            InitializeComponent();

            m_FrameHistoryChart = new FrameHistoryChart((int)ImageBox.Width, (int)ImageBox.Height);
            ImageBox.Source = m_FrameHistoryChart.WriteableBitmap;
        }

      
        public void SetTcpClient(ProfilerTcpClient client)
        {
            m_Client = client;

            client.OnRecievedProfileFrame += (n) => m_FrameHistoryChart.AddNode(n);

            client.OnRecievedProfileData += () => ImageBox.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                                          new Action(() =>
                                                                                          {
                                                                                              var summary = CalculateAverageFromChart();
                                                                                              var firstOrDefault = summary.FirstOrDefault(o => o.FrameType == ProfilerFrameType.Draw);
                                                                                             
                                                                                              if (firstOrDefault != null)
                                                                                              {
                                                                                                  var avg = firstOrDefault.Average;
                                                                                                  txtAvgTime.Text = String.Format("Median Draw Time: {0}ms", avg);
                                                                                              }

                                                                                              var firstOrDefaultUpdate = summary.FirstOrDefault(o => o.FrameType == ProfilerFrameType.Update);
                                                                                              if (firstOrDefaultUpdate != null)
                                                                                              {
                                                                                                  var avg = firstOrDefaultUpdate.Average;
                                                                                                  txtAvgUpdate.Text = String.Format("Median Update Time: {0}ms", avg);
                                                                                              }

                                                                                              m_FrameHistoryChart.DrawChart();
                                                                                              ImageBox.Source = m_FrameHistoryChart.WriteableBitmap;
                                                                                          }
                                                                             ));

            client.OnConnectedToServer += () => btnProfile.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                                          new Action(() =>
                                                                                                         {
                                                                                                             btnProfile.IsEnabled = true;
                                                                                                             btnProfile.Content = "Start Profiling";
                                                                                                         }
                                                                                      ));
        }

        private IEnumerable<FrameSummary> CalculateAverageFromChart()
        {
            var rv = new List<FrameSummary>();

            var groups = m_FrameHistoryChart.Nodes.Select(o => o.Children[0].FrameType).Distinct();

            foreach (var group in groups)
            {
                var times = new List<Double>(m_FrameHistoryChart.Nodes.Count);
                foreach (var node in m_FrameHistoryChart.Nodes)
                {
                    var c = node.Children[0];
                    if (c.FrameType == group)
                    {
                        times.Add(Double.Parse(c.TotalMilliseconds));
                    }
                }
                times.Sort();
         
                var avg =  times[times.Count / 2];

                rv.Add(new FrameSummary() { FrameType = group, Average = avg});
            }

            return rv;
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (m_Client.IsConnected)
            {
                if (m_IsCollecting == false)
                {
                    m_Client.SendStartProfilerMessage();
                  
                    m_IsCollecting = true;
                    btnProfile.Content = "Stop Profiling";
                }
                else
                {
                    m_Client.SendStopProfilerMessage();
                    m_IsCollecting = false;
                    m_FrameHistoryChart.ClearNodes();

                    btnProfile.Content = "Start Profiling";
                }
            }
        }

        private void ImageBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_FrameHistoryChart.Nodes.Count > 0)
            {
                var x = (int) e.GetPosition(ImageBox).X;
                var y = (int) e.GetPosition(ImageBox).Y;

               
                var node = m_FrameHistoryChart.SelectNode(x, y);

                // build the tree view
                if (node != null)
                {
                    treeview_currentFrame.Items.Clear();
                    AddNode(node, null, treeview_currentFrame);

                    // Calculate hotspots first serialize nodes
                    var lst = new List<HotspotNode>();
                    SerializeNodes(node, lst);
                    lst.Sort((a,b) => b.ElapsedTimeMilliseconds.CompareTo(a.ElapsedTimeMilliseconds));
                    lst = lst.Take(10).ToList();

                    Hotspots.Items.Clear();
                    foreach (var hotspotNode in lst)
                    {
                        var sp = new StackPanel() { Orientation = Orientation.Horizontal, Width = Hotspots.Width };

                        var tb = new TextBlock { Width = Hotspots.Width / 2, Text = hotspotNode.Name, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(1)};
                        var tb2 = new TextBlock { Width = Hotspots.Width / 2, Text = " :" + hotspotNode.ElapsedTimeMilliseconds.ToString("N4") + "ms", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(1) };

                        sp.Children.Add(tb);
                        sp.Children.Add(tb2);
                        Hotspots.Items.Add(sp);
                    }
                }
            }
        }

        private void SerializeNodes(IntervalNode root, List<HotspotNode> list)
        {
            if (!String.IsNullOrEmpty(root.TotalMilliseconds))
            {
                var time = double.Parse(root.TotalMilliseconds);

                foreach (var child in root.Children)
                {
                    var childTime = double.Parse(child.TotalMilliseconds);
                    time -= childTime;
                }

                list.Add(new HotspotNode() { Name = root.Name, ElapsedTimeMilliseconds = time});
            }
            foreach (var child in root.Children)
            {
                SerializeNodes(child, list);
            }
        }

        private void AddNode(IntervalNode node, TreeViewItem treeViewItem, TreeView treeView)
        {
            var tvi = new TreeViewItem();
            if (node.Name == null)
            {
                tvi.Header = "Root";
            }
            else
            {
                tvi.Header = String.Format("{0} :{1}ms", node.Name, node.TotalMilliseconds);// TODO fancy stack panel
            }

            tvi.IsExpanded = true;
            if (treeViewItem == null)
            {
                treeView.Items.Add(tvi);
            }
            else
            {
                treeViewItem.Items.Add(tvi);
            }

            foreach (var children in node.Children)
            {
                AddNode(children, tvi, treeView);
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_FrameHistoryChart == null)
                return;

            if (cmbTargetFps.SelectedItem == cmbiSixtyFps)
                m_FrameHistoryChart.TargetFps = 60;

            if (cmbTargetFps.SelectedItem == cmbiThirtyFps)
                m_FrameHistoryChart.TargetFps = 30;

            m_FrameHistoryChart.DrawChart();
        }
    }
}
