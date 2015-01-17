using System;
using System.Collections.Generic;

namespace MaglevProfiler
{
    public class RemotelyModifiableClassInfo
    {
        public String AssemblyName { get; set; }

        public String ClassName { get; set; }

        public List<RemotelyModifiablePropertyInfo> ClassProperties { get; set; }

        public RemotelyModifiableClassInfo()
        {
            
        }

        public RemotelyModifiableClassInfo(String className)
        {
            ClassName = className;
            ClassProperties = new List<RemotelyModifiablePropertyInfo>();
        }
    }
}
