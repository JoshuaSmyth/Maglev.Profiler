using System.Collections.Generic;
using System.Threading;

namespace MaglevProfiler
{
    public class ThreadedLogger
    {
        readonly Queue<IntervalNode> m_Queue = new Queue<IntervalNode>();
        private bool m_IsEnabled;

        public ThreadedLogger()
        {
            m_IsEnabled = true;
            var loggingThread = new Thread(ProcessQueue)
                                    {
                                        IsBackground = true
                                    };
            loggingThread.Start();
        }

        public void Exit()
        {
            m_IsEnabled = false;

        }

        void ProcessQueue()
        {
            while (m_IsEnabled)
            {
                Thread.Sleep(5000);

                lock (m_Queue)
                {
                    m_Queue.Clear();
                }
            }
        }

        public void QueueMessage(IntervalNode root)
        {
            lock (m_Queue)
            {
                m_Queue.Enqueue(root);
            }
        }
    }
}
