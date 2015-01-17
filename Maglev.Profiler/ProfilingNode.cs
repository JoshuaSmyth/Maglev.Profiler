using System;
using System.Diagnostics;
using System.Threading;
using MaglevProfiler.Pools;

namespace MaglevProfiler
{
    public sealed class ProfilingNode : IDisposable
    {
        private string m_Name;
        private ProfilerFrameType m_FrameType;
        private Int32 m_ManagedThreadId;
        private Stopwatch m_Stopwatch;
        private IntervalNode m_IntervalNode;
        private StopwatchPool m_StopwatchPool;
        private ProfilingNodePool m_OwnerPool;

        public IntervalNode IntervalNode
        {
            get { return m_IntervalNode; }
            set { m_IntervalNode = value; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public ProfilingNode()
        {
            
        }



        public void ResetProfilingNode(ProfilingNodePool ownerPool, string name, ProfilerFrameType frameType, IntervalNode intervalNode, StopwatchPool stopwatchPool)
        {
            m_OwnerPool = ownerPool;
            Name = name;
            m_FrameType = frameType;
            m_IntervalNode = intervalNode;
            m_StopwatchPool = stopwatchPool;
            m_IntervalNode.Name = Name;
            m_IntervalNode.FrameType = frameType;

            m_ManagedThreadId = Thread.CurrentThread.ManagedThreadId;
            m_IntervalNode.ManagedThreadId = m_ManagedThreadId;

            m_Stopwatch = m_StopwatchPool.RequestNode();
            m_Stopwatch.Start();
        }

        public void Dispose()
        {
           
            m_IntervalNode.HasFinished = true;
            m_Stopwatch.Stop();

            // Update the Interval node 
            var ts = m_Stopwatch.Elapsed;
            m_IntervalNode.TotalMilliseconds = ts.TotalMilliseconds.ToString();
            m_StopwatchPool.RestoreNode(m_Stopwatch);
            m_OwnerPool.RestoreNode(this);

        }
    }
}
