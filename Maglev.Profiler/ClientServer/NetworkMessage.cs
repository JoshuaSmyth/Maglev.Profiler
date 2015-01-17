using System;
using System.Text;

namespace MaglevProfiler.ClientServer
{
    public class NetworkMessage
    {
        public NetworkMessage(MessageType messageType)
        {
            MessageType = messageType;
            Data = "";
        }

        public NetworkMessage(MessageType messageType, String data)
        {
            MessageType = messageType;
            Data = data;
        }
        private static readonly UTF8Encoding m_Encoder = new UTF8Encoding();

        public MessageType MessageType { get; set; }

        public String Data { get; set; }

        public Int32 DataLength { get; set; }

        public byte[] ToBinaryForm()
        {
            var messageId = BitConverter.GetBytes((Int32)MessageType);
            var hasData = new byte[1];

            if (!string.IsNullOrEmpty(Data))
            {
                hasData[0] = 1;

                var data = m_Encoder.GetBytes(Data);
                var dataLength = BitConverter.GetBytes(Data.Length);

                var rv = new byte[messageId.Length + hasData.Length + dataLength.Length + data.Length];

                messageId.CopyTo(rv, 0);
                hasData.CopyTo(rv, 4);
                dataLength.CopyTo(rv, 5);
                data.CopyTo(rv, 9);

                return rv;
            }
            else
            {
                hasData[0] = 0;
               
                var rv = new byte[messageId.Length + hasData.Length];

                messageId.CopyTo(rv, 0);
                hasData.CopyTo(rv, messageId.Length);

                return rv;
            }
        }

        public static NetworkMessage FromBinaryForm(byte[] binaryForm)
        {
            var messageType = (MessageType) BitConverter.ToInt32(binaryForm, 0);

            var hasData = binaryForm[4];

            if (hasData == 1)
            {
                return BuildDataMessage(messageType, binaryForm);
            }
            else
            {
                return new NetworkMessage(messageType);
            }
        }

        private static NetworkMessage BuildDataMessage(MessageType messageType, byte[] binaryForm)
        {
            var datalength = BitConverter.ToInt32(binaryForm, 5);
            var myString = m_Encoder.GetString(binaryForm, 9, datalength);
            var rv = new NetworkMessage(messageType, myString) { DataLength = datalength };

            return rv;
        }
    }
}
