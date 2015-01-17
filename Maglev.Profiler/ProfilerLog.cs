using System;
using System.Collections.Generic;

namespace MaglevProfiler
{
    internal class ProfilerLog
    {
        public static List<String> Log = new List<string>(64);

        public static void AddString(string log)
        {
            Log.Add(log);
        }

        public static void Clear()
        {
            Log.Clear();
        }
    }
}
