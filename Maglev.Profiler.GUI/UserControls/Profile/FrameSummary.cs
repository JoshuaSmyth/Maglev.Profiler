using System;
using MaglevProfiler;

namespace TinyFrog.Profiler.Gui.UserControls.Profile
{
    public class FrameSummary
    {
        public ProfilerFrameType FrameType { get; set; }

        public Double Average { get; set; }
    }

    public class HotspotNode
    {
        public String Name { get; set; }
        public Double ElapsedTimeMilliseconds { get; set; }
    }
}
