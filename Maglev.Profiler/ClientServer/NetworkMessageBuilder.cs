using System.Linq;

namespace MaglevProfiler.ClientServer
{
    public class NetworkMessageBuilder
    {
        private readonly RemoteModifier m_RemoteModifier;
        private readonly Serializer m_Serializer;

        public NetworkMessageBuilder()
        {
            m_RemoteModifier = new RemoteModifier();
            m_Serializer = new Serializer();
        }

        public NetworkMessage BuildHandshakeMessage()
        {
            return new NetworkMessage(MessageType.Handshake);
        }

        public NetworkMessage BuildProfilerDataMessage(string data)
        {
            return new NetworkMessage(MessageType.RecieveProfileFrames, data);
        }

        public NetworkMessage BuildStartProfilerMessage()
        {
            return new NetworkMessage(MessageType.StartProfiler);
        }

        public NetworkMessage BuildStopProfilerMessage()
        {
            return new NetworkMessage(MessageType.StopProfiler);
        }

        public NetworkMessage BuildRequestGlobalLog()
        {
            return new NetworkMessage(MessageType.RequestGlobalLog);
        }

        public NetworkMessage BuildRecieveGlobalLog()
        {
            var log = Profiler.GetGlobalLog();
            return new NetworkMessage(MessageType.RecieveGlobalLog, log);
        }

        public NetworkMessage BuildRequestRemoteModifiableClassesMetaData()
        {
            return new NetworkMessage(MessageType.RequestRemotelyModifiableClassesMetaData);
        }

        public NetworkMessage BuildRecieveRemoteModifiableClassesMetaData()
        {
            var classes = m_RemoteModifier.GetAllRemotelyModifiableClasses().ToList();
            if (classes.Any())
            {
                var serialForm = m_Serializer.Write(classes);
                return new NetworkMessage(MessageType.RecieveRemotelyModifiableClassesMetaData, serialForm);
            }
            return new NetworkMessage(MessageType.RecieveRemotelyModifiableClassesMetaData);
        }

        public NetworkMessage BuildModifyClassMessage(RemotelyModifiableClassInfo classInfo)
        {
            var serialForm = m_Serializer.Write(classInfo); // SerializationHelper.Serialize(classInfo);

            var message = new NetworkMessage(MessageType.RemotelyModifyClass, serialForm);
            return message;
        }
    }
}
