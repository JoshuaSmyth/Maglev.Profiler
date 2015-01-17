using System;
using System.Collections.Generic;

namespace MaglevProfiler
{
    public class RemotelyModifiablePropertyInfo
    {
        public String PropertyName { get; set; }

        public String PropertyType { get; set; }

        public String PropertyValue { get; set; }

        public List<String> PotentialValues { get; set; }

        public Boolean IsEnum { get { return EnumCount > 0; } }

        public Int32 EnumCount { get; set; }

        public bool IsStatic { get; set; }

        public RemotelyModifiablePropertyInfo()
        {
            
        }

        public RemotelyModifiablePropertyInfo(String name, 
                                              String type, 
                                              String value)
        {
            PropertyName = name;
            PropertyType = type;
            PropertyValue = value;
        }

        /// <summary>
        /// For enums
        /// </summary>
        public RemotelyModifiablePropertyInfo(String name,
                                      String type,
                                      String currentValue,
                                      Int32 enumCount,
                                      List<String> potentialValues)
        {
            PropertyName = name;
            PropertyType = type;
            PropertyValue = currentValue;
            PotentialValues = potentialValues;
            EnumCount = enumCount;
        }
    }
}
