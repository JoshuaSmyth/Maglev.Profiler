using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MaglevProfiler.ClientServer
{
    public class ProfilerTcpServer : IDisposable
    {
        private readonly TcpListener m_TcpListener;
        private readonly Thread m_ListenThread;
        private bool m_bIsRunning;
        private NetworkStream m_CurrentStream;

        private NetworkMessageBuilder m_NetworkMessageBuilder;

        private RemoteModifier m_RemoteModifier;
        private Serializer m_Serializer;

        public ProfilerTcpServer(Int32 portNumber)
        {
            try
            {
                m_Serializer = new Serializer();
                m_NetworkMessageBuilder = new NetworkMessageBuilder();
                m_RemoteModifier = new RemoteModifier();
                m_bIsRunning = true;
                this.m_TcpListener = new TcpListener(IPAddress.Any, portNumber);
                this.m_ListenThread = new Thread(new ThreadStart(ListenForClients));
                this.m_ListenThread.Start();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Exit()
        {
            if (m_CurrentStream != null)
                m_CurrentStream.Close();
            
            m_TcpListener.Stop();
            m_bIsRunning = false;
        }

        private void ListenForClients()
        {
            m_TcpListener.Start();

            while (m_bIsRunning)
            {
                try
                {
                    //blocks until a client has connected to the server
                    var client = m_TcpListener.AcceptTcpClient(); 
                    
                    //create a thread to handle communication with connected client
                    var clientThread = new Thread(HandleClientComm);
                    clientThread.Start(client);

                    // Send handshake
                    var message = m_NetworkMessageBuilder.BuildHandshakeMessage();
                    SendMessage(message, client);
                }
                catch (SocketException socketException)
                {
                    if (socketException.SocketErrorCode == SocketError.Interrupted)
                        return;

                    throw;
                }
            }
        }

        private void SendMessage(NetworkMessage message, TcpClient client)
        {
            var clientStream = client.GetStream();
            byte[] buffer = message.ToBinaryForm();

            Console.WriteLine(String.Format("Length:{0}",buffer.Length));

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        private void HandleClientComm(object client)
        {
            var tcpClient = (TcpClient)client;
            m_CurrentStream = tcpClient.GetStream();

            var messageData = new byte[4096];

            while (m_bIsRunning)
            {
                var bytesRead = 0;

                try
                {
                    bytesRead = m_CurrentStream.Read(messageData, 0, 4096); //blocks until a client sends a message
                }
                catch(Exception exception)
                {
                    // A socket Exception has occurred TODO WriteFrameLog exception
                    break;
                }

                // The client has disconnected from the server
                if (bytesRead == 0) 
                    break;
                
                var message = NetworkMessage.FromBinaryForm(messageData);
                HandleMessage(tcpClient, message);
            }

            tcpClient.Close();
        }

        public void Dispose()
        {
           m_TcpListener.Stop();
        }

        private void HandleMessage(TcpClient client, NetworkMessage message)
        {
            if (message.MessageType == MessageType.StartProfiler)
            {
                MaglevProfiler.Profiler.Enable();
                return;
            }

            if (message.MessageType == MessageType.StopProfiler)
            {
                MaglevProfiler.Profiler.Disable();

                // Wait until the Profiler has finished its frame                  
                while (MaglevProfiler.Profiler.HasFinished() == false)
                {
                }

                // TODO This string could do with a bit of compression as the data can get > 50kb
                var returnMessage = m_NetworkMessageBuilder.BuildProfilerDataMessage(MaglevProfiler.Profiler.Serialize());
                SendMessage(returnMessage, client);
                return;
            }

            if (message.MessageType == MessageType.RequestRemotelyModifiableClassesMetaData)
            {
                var returnMessage = m_NetworkMessageBuilder.BuildRecieveRemoteModifiableClassesMetaData();
                SendMessage(returnMessage, client);
                return;
            }

            if (message.MessageType == MessageType.RemotelyModifyClass)
            {
                var classInfo = m_Serializer.ReadRemotelyModifyClassInfo(message.Data);// m_ //SerializationHelper.Deserialize<RemotelyModifiableClassInfo>(message.Data);
                m_RemoteModifier.UpdateRemotelyModifiableClass(classInfo);
                return;
            }

            if (message.MessageType == MessageType.RequestGlobalLog)
            {
                var returnMessage = m_NetworkMessageBuilder.BuildRecieveGlobalLog();
                SendMessage(returnMessage, client);
                return;
            }
        }
    }
}
