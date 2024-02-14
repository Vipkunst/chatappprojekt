using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net.IO
{
    class PacketReader : BinaryReader
    {
        private NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }
        public string ReadMessage()
        {
            byte[] msgBuffer;
            var length = ReadInt32();
            msgBuffer = new byte[length];
            _ns.Read(msgBuffer, 0, length);

            var msg = Encoding.ASCII.GetString(msgBuffer);
            return msg;
        }
        public string ReadRecipient()
        {
            byte[] recipientBuffer;
            var recipientlength = ReadInt32();
            recipientBuffer = new byte[recipientlength];
            _ns.Read(recipientBuffer, 0, recipientlength);
        
            var recipient = Encoding.ASCII.GetString(recipientBuffer);
            return recipient;
        }
        public string ReadSender()
        {
            byte[] senderBuffer;
            var senderlength = ReadInt32();
            senderBuffer = new byte[senderlength];
            _ns.Read(senderBuffer, 0, senderlength);

            var sender = Encoding.ASCII.GetString(senderBuffer);
            return sender;
        }
    }
}
