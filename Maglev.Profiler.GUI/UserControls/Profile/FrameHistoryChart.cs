using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaglevProfiler;

namespace TinyFrog.Profiler.Gui.UserControls.Profile
{
    public class FrameHistoryChart
    {
        private readonly List<IntervalNode> m_nodes = new List<IntervalNode>();
        private const int barwidth = 8;

        public FrameHistoryChart(int width, int height)
        {
            Width = width;
            Height = height;
            WriteableBitmap = new WriteableBitmap((int) width,(int) height,96,96,PixelFormats.Pbgra32,null);

            TargetFps = 60;
        }

        public WriteableBitmap WriteableBitmap { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        private int m_SelectedIndex = -1;

        public List<IntervalNode> Nodes
        {
            get { return m_nodes; }
        }

        public int TargetFps { get; set; }

        public void AddNode(IntervalNode n)
        {
            Nodes.Add(n);
        }

        public void ClearNodes()
        {
            Nodes.Clear();
        }

        public IntervalNode SelectNode(int x, int y)
        {
            var index = x/barwidth;
            if (Nodes.Count >= index)
            {
                m_SelectedIndex = index;
                DrawChart();
                return Nodes[index];
            }
            return null;
        }

        public void DrawChart()
        {
            WriteableBitmap.Clear();
          //  WriteableBitmap.FillRectangle(0, 0, (Int32)WriteableBitmap.Width, (Int32)WriteableBitmap.Height, Color.FromRgb(168, 217, 255));

            // Work out max height
            var nodeTimes = (from n in Nodes select Double.Parse(n.Children.First().TotalMilliseconds)).ToList();
            var scalednodes = nodeTimes.Select(o => (o / (1000.0f / TargetFps)) * Height).ToList();

            var i = 0;
            var x = 0;
            foreach (var node in scalednodes)
            {
                // Draw bar
                for (int w = x; w < x + barwidth; w++)
                {
                    var color = Color.FromRgb(0, 64, 212);
                    if (i %2 == 0)
                        color = Color.FromRgb(0, 192, 212);
    
                    if (node > Height)    // Didn't come in under the target frame limit
                        color = Color.FromRgb(255, 0, 0);
                
                    WriteableBitmap.DrawLine(w, Height - 1, w, (int)(Height - (node/2)), color);
                }

                x+=barwidth;
                i++;
            }

            if (m_SelectedIndex >= scalednodes.Count)
                m_SelectedIndex = 0;

            // Draw outline for selected node
            if (m_SelectedIndex > 0)
            {
                var selectedNode = scalednodes[m_SelectedIndex];
                var h = (int)(Height - (selectedNode / 2));

                x = m_SelectedIndex*barwidth;
        
                WriteableBitmap.DrawLine(x - 1, Height - 1, x - 1, h, Color.FromRgb(255, 255, 0));
                WriteableBitmap.DrawLine(x, Height - 1, x, h, Color.FromRgb(255, 255, 0));

                WriteableBitmap.DrawLine(x + barwidth + 1, Height - 1, x + barwidth + 1, h, Color.FromRgb(255, 255, 0));
                WriteableBitmap.DrawLine(x + barwidth, Height - 1, x + barwidth, h, Color.FromRgb(255, 255, 0));

                WriteableBitmap.DrawLine(x - 1, h, x + barwidth + 1, h, Color.FromRgb(255, 255, 0));
                WriteableBitmap.DrawLine(x - 1, h + 1, x + barwidth + 1, h + 1, Color.FromRgb(255, 255, 0));
            }

            // Draw midline
            WriteableBitmap.DrawLine(0, Height / 2, Width, Height / 2, Color.FromRgb(64, 64, 64));

            // TODO Draw the scale
        }
    }
}
