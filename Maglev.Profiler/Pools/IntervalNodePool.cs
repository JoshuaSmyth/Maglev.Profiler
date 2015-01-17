using System;
using System.Collections.Generic;

namespace MaglevProfiler.Pools
{
    public class IntervalNodePool
    {
        private const Int32 InitalQueueSize = 16384;

        readonly Queue<IntervalNode> m_Nodes = new Queue<IntervalNode>(InitalQueueSize); 

        public IntervalNodePool()
        {
            PopulateQueue();
        }

        private void PopulateQueue()
        {
            for (int i = 0; i < InitalQueueSize; i++)
            {
                m_Nodes.Enqueue(new IntervalNode(null));
            }
        }

        public IntervalNode RequestNode(IntervalNode parent)
        {
            lock (m_Nodes)
            {
                if (m_Nodes.Count == 0)
                    PopulateQueue();

                var node = m_Nodes.Dequeue();
                node.Parent = parent;

                return node;
            }
        }

        public void RestoreNode(IntervalNode node)
        {
            lock (m_Nodes)
            {
                ResetNode(node);

                m_Nodes.Enqueue(node);
            }
        }

        private static void ResetNode(IntervalNode node)
        {
            node.Name = "Root";
            node.TotalMilliseconds = "";
            node.Children.Clear();
            node.Parent = null;
            node.HasFinished = false;
            node.Logs.Clear();
        }
    }
}
