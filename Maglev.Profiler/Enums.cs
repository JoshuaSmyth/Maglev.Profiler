namespace MaglevProfiler
{
    public enum MessageType
    {
        None = 0,
        Handshake = 1,
        StartProfiler = 2,
        StopProfiler = 3,
        UpdateRemotelyModifiableClass = 4,
        RecieveProfileFrames = 5,
        RequestRemotelyModifiableClassesMetaData = 6,
        RecieveRemotelyModifiableClassesMetaData = 7,
        RemotelyModifyClass = 8,
        RequestGlobalLog = 9,
        RecieveGlobalLog = 10
    }

    public enum ProfilerFrameType
    {
        NoneSpecified = 0,
        Draw = 1,
        Update = 2
    }
}
