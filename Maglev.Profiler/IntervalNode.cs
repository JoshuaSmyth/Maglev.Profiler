using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MaglevProfiler
{

    [Serializable]
    public class IntervalNode
    {
        private string m_Name;
        private ProfilerFrameType m_FrameType;
        private Boolean m_HasFinished = false;
        private Int32 m_ManagedThreadId;
        private IntervalNode m_Parent;
        private readonly List<IntervalNode> m_Children = new List<IntervalNode>(42);
        private List<String> m_Logs = new List<string>(42);
        private String m_Milliseconds;

        public IntervalNode()
        {
            // Parameterless constructor required for serialization
            m_Name = "Root";
        }

        public IntervalNode(IntervalNode parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// The total time elapsed for this node inclusive of the timing for any children the node has.
        /// Note: Using a string because conversions with Timespan round to the nearest millisecond and we require sub millisecond accuracy
        /// </summary>
        public String TotalMilliseconds
        {
            get { return m_Milliseconds; }
            set { m_Milliseconds = value; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public int ManagedThreadId
        {
            get { return m_ManagedThreadId; }
            set { m_ManagedThreadId = value; }
        }

        [IgnoreDataMember]
        [XmlIgnore]
        public bool HasFinished
        {
            get { return m_HasFinished; }
            set { m_HasFinished = value; }
        }

        [IgnoreDataMember]
        [XmlIgnore]
        public IntervalNode Parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        public List<IntervalNode> Children
        {
            get { return m_Children; }
         //   set { m_Children = value; }
        }

        public List<string> Logs
        {
            get { return m_Logs; }
            set { m_Logs = value; }
        }

        public ProfilerFrameType FrameType
        {
            get { return m_FrameType; }
            set { m_FrameType = value; }
        }


        public void AddChild(IntervalNode newNode)
        {
            lock(m_Children)
            {
                m_Children.Add(newNode);
            }
        }

        public void Log(String log)
        {
            m_Logs.Add(log);
        }
    }
}
