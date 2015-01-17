using System;
using System.Diagnostics;

namespace MaglevProfiler
{
    public class LogTime : IDisposable
    {
        private readonly string m_Tracename;
        private readonly Stopwatch m_Stopwatch = new Stopwatch();
        public LogTime(String tracename)
        {
            m_Tracename = tracename;
            m_Stopwatch.Start();
        }

        public LogTime(String tracename, params object[] args)
        {
            m_Tracename = String.Format(tracename, args);
            m_Stopwatch.Start();
        }

        public void Dispose()
        {
            m_Stopwatch.Stop();

            MaglevProfiler.Profiler.WriteLog("{0}: Total Millisenconds: {1}ms", m_Tracename, m_Stopwatch.ElapsedMilliseconds);
        }
    }
}
