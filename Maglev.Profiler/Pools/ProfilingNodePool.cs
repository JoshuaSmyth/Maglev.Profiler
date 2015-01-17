using System;
using System.Collections.Generic;

namespace MaglevProfiler.Pools
{
    public class ProfilingNodePool
    {
        private const Int32 InitalQueueSize = 16384;

        readonly Queue<ProfilingNode> m_Nodes = new Queue<ProfilingNode>(InitalQueueSize); 

        public ProfilingNodePool()
        {
            PopulateQueue();
        }

        private void PopulateQueue()
        {
            for (int i = 0; i < InitalQueueSize; i++)
            {
                m_Nodes.Enqueue(new ProfilingNode());
            }
        }

        public ProfilingNode RequestNode()
        {
            lock (m_Nodes)
            {
                if (m_Nodes.Count == 0)
                    PopulateQueue();

                var node = m_Nodes.Dequeue();

                return node;
            }
        }

        public void RestoreNode(ProfilingNode node)
        {
            lock (m_Nodes)
            {
                node.Name = "";
                node.IntervalNode = null;
                m_Nodes.Enqueue(node);
            }
        }

        public ProfilingNode RequestNode(string name, ProfilerFrameType frameType, IntervalNode newNode, StopwatchPool stopwatchPool)
        {
            var node = RequestNode();
            node.ResetProfilingNode(this, name, frameType, newNode, stopwatchPool);
            return node;
        }
    }
}
