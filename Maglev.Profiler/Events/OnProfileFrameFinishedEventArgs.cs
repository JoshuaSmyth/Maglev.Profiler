using System;

namespace MaglevProfiler.Events
{
    public class OnProfileFrameFinishedEventArgs : EventArgs
    {
        private IntervalNode m_IntervalTreeRoot;

        public OnProfileFrameFinishedEventArgs(IntervalNode intervalRoot)
        {
            m_IntervalTreeRoot = intervalRoot;
        }
        
        public IntervalNode IntervalTreeRoot
        {
            get { return m_IntervalTreeRoot; }
            set { m_IntervalTreeRoot = value; }
        }
    }
}
