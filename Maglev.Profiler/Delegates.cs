using System;
using System.Collections.Generic;

namespace MaglevProfiler
{
    public delegate void OnRecievedProfileFrame(IntervalNode intervalRoot);
    public delegate void OnRecievedRemoteClassMetaData(List<RemotelyModifiableClassInfo> classes);

    public delegate void OnRecievedGlobalLogData(String data);

    public delegate void OnRecievedProfileData();
    public delegate void OnConnectedToServer();
    public delegate void OnDisconnectedFromServer();
    public delegate void OnCommunicationError(Exception exception);    
}
