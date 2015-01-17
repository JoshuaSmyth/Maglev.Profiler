using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MaglevProfiler.Pools
{
    public class StopwatchPool
    {
        private const Int32 InitalQueueSize = 16384;

        readonly Queue<Stopwatch> m_Nodes = new Queue<Stopwatch>(InitalQueueSize);

        public StopwatchPool()
        {
            PopulateQueue();
        }

        private void PopulateQueue()
        {
            for (int i = 0; i < InitalQueueSize; i++)
            {
                m_Nodes.Enqueue(new Stopwatch());
            }
        }

        public Stopwatch RequestNode()
        {
            lock (m_Nodes)
            {
                if (m_Nodes.Count == 0)
                    PopulateQueue();

                var node = m_Nodes.Dequeue();

                return node;
            }
        }

        public void RestoreNode(Stopwatch stopwatch)
        {
            lock (m_Nodes)
            {
                stopwatch.Reset();

                m_Nodes.Enqueue(stopwatch);
            }
        }
    }
}
