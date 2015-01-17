using System;
using System.Collections.ObjectModel;
using System.Threading;
using MaglevProfiler.ClientServer;
using MaglevProfiler.Pools;

namespace MaglevProfiler
{
    public static class Profiler
    {
        private static ProfilerTcpServer m_ProfilerTcpServer;
        static Profiler()
        {
            IntervalTree = new IntervalTree();
        }

        private const Int32 MaxPreviousFrames = 300;

        private static bool m_bIsActive = false;
        private static bool m_bWillActivateNextFrame = false;
        private static bool m_bWillDeactivateNextFrame = false;

        public static IntervalTree IntervalTree { get; set; }

        private static readonly Collection<IntervalNode> FrameHistory = new Collection<IntervalNode>();

        public static event OnRecievedProfileFrame OnProfileFrameFinished;

        private static ThreadedLogger m_Logger;

        private static readonly Serializer Serializer = new Serializer();

        private static IntervalNodePool IntervalNodePool = new IntervalNodePool();

        private static StopwatchPool StopwatchPool = new StopwatchPool();

        private static ProfilingNodePool ProfilingNodePool = new ProfilingNodePool();

        private static ProfilerLog ProfilerLog = new ProfilerLog();

        /// <summary>
        /// Must be called when the application quits
        /// </summary>
        public static void Exit()
        {
#if ENABLE_PROFILER
            m_Logger.Exit();

            if (m_ProfilerTcpServer != null)
                m_ProfilerTcpServer.Exit();
#endif
        }

        public static void Initalize(Int32 portnumber)
        {
#if ENABLE_PROFILER    // TODO Enable the profiler for iOS
            m_Logger = new ThreadedLogger();
            m_ProfilerTcpServer = new ProfilerTcpServer(portnumber); // Start the TCP Server
            Console.WriteLine("Started TCP Server");
#endif
        }

        public static string Serialize()
        {
            return Serializer.Write(FrameHistory);
        }

        /// <summary>
        /// Enables the Profiler
        /// </summary>
        /// <param name="enableMidFrame">Determines if the Profiler starts straight away (potentially mid frame) or waits for the next frame</param>
        public static void Enable(bool enableMidFrame = true)
        {
#if ENABLE_PROFILER
            m_bWillActivateNextFrame = true;

            m_bIsActive = enableMidFrame;
            FrameHistory.Clear();
#endif
        }

        internal static void Disable()
        {
            m_bWillDeactivateNextFrame = true;
        }

        private static ProfilingNode ProfileSection(String name, ProfilerFrameType frameType)
        {
            if (m_bIsActive == false)
                return null;

            var currentNode = IntervalTree.CurrentNode;

            var newNode = IntervalNodePool.RequestNode(currentNode); //new IntervalNode(currentNode);

            if (currentNode == null || currentNode.HasFinished)
            {
                if (currentNode != null)
                {
                    // Go back up the stack until we find a node that hasn't finished
                    while (newNode.Parent == null || newNode.Parent.HasFinished == true)
                    {
                        newNode.Parent = currentNode.Parent;
                        currentNode = currentNode.Parent;
                    }

                    currentNode.AddChild(newNode);
                    IntervalTree.CurrentNode = newNode;
                }
                else
                {
                    // Add a new root node
                    IntervalTree.IntervalRoot = newNode;
                    IntervalTree.CurrentNode = newNode;
                }
            }
            else
            {
                // Current node hasn't finished that means it is either a thread or a child of the current node
                var threadId = Thread.CurrentThread.ManagedThreadId;

                if (currentNode.ManagedThreadId != 0 && currentNode.ManagedThreadId != threadId)
                {
                    // find node with the thread   
                    var n = IntervalTree.FindNodeByThreadId(threadId);
                    if (n == null)
                    {
                        currentNode.AddChild(newNode);
                        IntervalTree.CurrentNode = newNode;
                    }
                    else
                    {
                        n.AddChild(newNode);
                    }
                }
                else
                {
                    currentNode.AddChild(newNode);
                    IntervalTree.CurrentNode = newNode;
                }
            }

            var node = ProfilingNodePool.RequestNode(name, frameType, newNode, StopwatchPool);// new ProfilingNode(name, frameType, newNode, StopwatchPool);
            return node;
        }

        public static ProfilingNode ProfileSection(String name)
        {
#if ENABLE_PROFILER
            return ProfileSection(name, ProfilerFrameType.NoneSpecified);
#else
            return null;
#endif
        }

        private static void UpdateListeners()
        {
            if (IntervalTree.IntervalRoot == null)
                return;

            if (OnProfileFrameFinished != null)
                OnProfileFrameFinished.Invoke(IntervalTree.IntervalRoot);
        }


       
        public static ProfilingNode ProfileFrame(ProfilerFrameType profilerFrameType)
        {
#if ENABLE_PROFILER
            // Check if we are queued to activate
            if (m_bWillActivateNextFrame)
            {
                m_bIsActive = true;
                m_bWillActivateNextFrame = false;
            }

            // Check if we are queued to deactivate
            if (m_bWillDeactivateNextFrame)
            {
                m_bIsActive = false;
                m_bWillDeactivateNextFrame = false;
            }

            if (m_bIsActive == false)
                return null;

            UpdateListeners();

            // Create the new frame
            IntervalTree.IntervalRoot = IntervalNodePool.RequestNode(null);
            IntervalTree.CurrentNode = IntervalTree.IntervalRoot;

            // Store a maximum history
            if (FrameHistory.Count > MaxPreviousFrames)
            {
                var oldTree = FrameHistory[0];
                RestoreIntervalNodesToPool(oldTree);
                FrameHistory.RemoveAt(0);
            }
            FrameHistory.Add(IntervalTree.IntervalRoot);

            if (m_Logger != null)
                m_Logger.QueueMessage(IntervalTree.IntervalRoot);

            var name = "Unknown";
            if (profilerFrameType == ProfilerFrameType.Draw)
                name = "Draw";
            if (profilerFrameType == ProfilerFrameType.Update)
                name = "Update";

            return ProfileSection(name, profilerFrameType);
#else
            return null;
#endif
        }

        public static void RestoreIntervalNodesToPool(IntervalNode node)
        {
            foreach (var child in node.Children)
            {
                RestoreIntervalNodesToPool(child);
            }
            IntervalNodePool.RestoreNode(node);
        }

        public static void WriteFrameLog(IntervalNode node, String message)
        {
            if (m_bIsActive)
            {
                node.Log(message);
            }
        }


        public static void WriteFrameLog(String message)
        {
            if (m_bIsActive)
            {
                if (IntervalTree.CurrentNode == null)
                    throw new Exception("No active profile to log to");

                IntervalTree.CurrentNode.Log(message);
            }
        }

        internal static bool HasFinished()
        {
            return !m_bIsActive;
        }

        public static void Stop()
        {
            m_ProfilerTcpServer.Dispose();
        }

        public static void WriteLog(String log, params object[] args)
        {
            Console.WriteLine(log, args);

#if ENABLE_PROFILER
            ProfilerLog.AddString(String.Format(log, args));
#endif
        }

        public static void ClearGlobalLog()
        {
            ProfilerLog.Clear();
        }

        public static String GetGlobalLog()
        {
            return String.Join(Environment.NewLine, ProfilerLog.Log);
        }

        public static LogTime LogTime(string tracename)
        {
            return new LogTime(tracename);
        }

        public static LogTime LogTime(string tracename, params Object[] args)
        {
            return new LogTime(tracename, args);
        }
    }
}
