using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MaglevProfiler.ClientServer
{
    public class ProfilerTcpClient
    {
        public event OnRecievedRemoteClassMetaData OnRecievedRemoteClassMetaData;
        public event OnRecievedGlobalLogData OnRecievedGlobalLogData;
        public event OnRecievedProfileFrame OnRecievedProfileFrame;
        public event OnRecievedProfileData OnRecievedProfileData;
        
        public event OnConnectedToServer OnConnectedToServer;
        public event OnCommunicationError OnCommunicationError;


        private Thread m_ListenThread;
        private NetworkStream m_MyStream;
        private TcpClient m_MyClient;
        private byte[] m_DataBuffer;
        private readonly NetworkMessageBuilder m_NetworkMessageBuilder;

        private readonly Serializer m_Serializer;

        public bool IsConnected { get; set; }

     
        public ProfilerTcpClient()
        {
            IsConnected = false;
            m_NetworkMessageBuilder = new NetworkMessageBuilder();
            m_Serializer = new Serializer();
        }

        private void ListenThread()
        {
            while (IsConnected)
            {
             
                Array.Clear(m_DataBuffer, 0, m_DataBuffer.Length); // Ensures we don't have excess data hanging around

                try
                {
                    var lData = m_MyStream.Read(m_DataBuffer, 0, m_DataBuffer.Length);

                    // No data recieved. Disconnection has occured.
                    if (lData == 0)
                        return;

                    var hasData = m_DataBuffer[4];
                    if (hasData == 1)
                    {
                        var messageLength = BitConverter.ToInt32(m_DataBuffer, 5) + 9; // +9 because that is the header size
                        if (messageLength > lData)
                        {
                            // Need to keep reading
                            var totalBytesRead = lData;
                            var buffer = new byte[4096];
                            var ms = new MemoryStream(messageLength);
                            ms.Write(m_DataBuffer,0,lData);
                            while (messageLength > totalBytesRead)
                            {
                                var bytesRead = m_MyStream.Read(buffer, 0, buffer.Length);
                                ms.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                Array.Clear(buffer, 0, buffer.Length); // Ensures we don't have excess data hanging around
                            }

                            Debug.Assert(messageLength == totalBytesRead);

                            var bytes = ms.ToArray();
                            var message = NetworkMessage.FromBinaryForm(bytes);
                            HandleMessage(message);
                        }
                        else
                        {
                            var message = NetworkMessage.FromBinaryForm(m_DataBuffer);
                            HandleMessage(message);
                        }
                    }
                    else
                    {
                        var message = NetworkMessage.FromBinaryForm(m_DataBuffer);
                        HandleMessage(message);
                    }
                }
                catch (Exception exception)
                {
                    if (IsConnected == true) // Only report errors if we are connected
                    {
                        if (OnCommunicationError != null)
                            this.OnCommunicationError.Invoke(new ApplicationException("TCP Error", exception));

                        // Abort the thread
                        IsConnected = false;
                    }
                }
            }
        }

        private void HandleMessage(NetworkMessage message)
        {
            if (message.MessageType == MessageType.Handshake)
            {
                if (OnConnectedToServer != null)
                    OnConnectedToServer.Invoke();
                return;
            }

            if (message.MessageType == MessageType.RecieveProfileFrames)
            {
                try
                {
                    RecieveProfileFramesMessage(message);
                    return;
                }
                catch (Exception e)
                {
                    throw new UnableToDecodeMessageException("RecieveProfileFramesMessage", e);
                }
            }

            if (message.MessageType == MessageType.RecieveRemotelyModifiableClassesMetaData)
            {
                try
                {
                    RecieveRemotelyModifiableClassesMetaData(message);
                    return;
                }
                catch (Exception e)
                {
                    throw new UnableToDecodeMessageException("RecieveRemotelyModifiableClassesMetaData", e);
                }
            }

            if (message.MessageType == MessageType.RecieveGlobalLog)
            {
                if (OnRecievedGlobalLogData != null)
                {
                    OnRecievedGlobalLogData.Invoke(message.Data);
                }
                return;
            }

            throw new UnknownMessageException("Unknown message recieved");
        }

        private void RecieveRemotelyModifiableClassesMetaData(NetworkMessage message)
        {
            if (message.DataLength == 0)
                return;

            var results = m_Serializer.ReadAllRemotelyModifiableClassInfo(message.Data);

            if (OnRecievedRemoteClassMetaData != null)
            {
                OnRecievedRemoteClassMetaData.Invoke(results);
            }
        }

        private  void RecieveProfileFramesMessage(NetworkMessage message)
        {
            try
            {
                var node = m_Serializer.ReadFrameHistory(message.Data);
                foreach (var n in node)
                {
                    if (n == null)  // TODO work out why this first frame is null
                        continue;
                            
                    if (OnRecievedProfileFrame != null)
                        OnRecievedProfileFrame.Invoke(n);
                }

                if (OnRecievedProfileData != null)
                    OnRecievedProfileData.Invoke();
            }
            catch (Exception exception)
            {
                if (OnCommunicationError != null)
                    OnCommunicationError.Invoke( new ApplicationException("Error processing Frame data",exception));
            }
        }

        public void Disconnect()
        {
            m_MyClient.Close();
            IsConnected = false;
        }

        public bool Connect(string ipAddress, string portNumber)
        {
            try
            {
                m_MyClient = new TcpClient(ipAddress, Int32.Parse(portNumber)) { ReceiveBufferSize = 2048 * 2048 };                 // 1 mb
                m_MyStream = m_MyClient.GetStream();
                m_DataBuffer = new byte[8192];

                m_ListenThread = new Thread(ListenThread);
                m_ListenThread.Start();

               // Profiler.ClearGlobalLog();
                IsConnected = true;
                return true;
            }
            catch (SocketException exception)
            {
                if (OnCommunicationError != null)
                    OnCommunicationError.Invoke(exception);
            }
            return false;
        }

        private void SendMessage(NetworkMessage message)
        {
            if (m_MyClient == null)
            {
                if (OnCommunicationError != null)
                    OnCommunicationError.Invoke(new ApplicationException("Not Connected to a Server"));
                return;
            }

            var messageData = message.ToBinaryForm();
            m_MyStream.Write(messageData, 0, messageData.Length);
        }

        public void SendStartProfilerMessage()
        {
            var message = m_NetworkMessageBuilder.BuildStartProfilerMessage();
            SendMessage(message);
        }

        public void SendStopProfilerMessage()
        {
            var message = m_NetworkMessageBuilder.BuildStopProfilerMessage();
            SendMessage(message);
        }

        public void SendRequestRemoteClassMetaDataMessage()
        {
            var message = m_NetworkMessageBuilder.BuildRequestRemoteModifiableClassesMetaData();
            SendMessage(message);
        }

        public void SendModifyPropertyMessage(RemotelyModifiableClassInfo classInfo)
        {
            var message = m_NetworkMessageBuilder.BuildModifyClassMessage(classInfo);
            SendMessage(message);
        }

        public void SendRequestGlobalLogMessage()
        {
            var message = m_NetworkMessageBuilder.BuildRequestGlobalLog();
            SendMessage(message);
        }
    }
}
