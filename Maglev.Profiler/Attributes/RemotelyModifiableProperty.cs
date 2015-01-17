using System;

namespace MaglevProfiler.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RemotelyModifiableClass : System.Attribute
    {
        
    }


    // TODO Allow Note attribute parameter

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RemotelyModifiableProperty : System.Attribute
    {

    }
}
