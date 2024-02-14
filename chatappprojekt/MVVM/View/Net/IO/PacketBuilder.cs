using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net.IO
{
    class PacketBuilder
    {
        MemoryStream _ms;
        
        public PacketBuilder()
        { 
            _ms = new MemoryStream();
        }
        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }
        public void WriteRecipient(string recipient)
        {
            var recipientLength = recipient.Length;
            _ms.Write(BitConverter.GetBytes(recipientLength));
            _ms.Write(Encoding.ASCII.GetBytes(recipient));
        }
        public void WriteSender(string sender)
        {
            var senderLength = sender.Length;
            _ms.Write(BitConverter.GetBytes(senderLength));
            _ms.Write(Encoding.ASCII.GetBytes(sender));
        }
        public void WriteMessage(string msg) 
        {
            var msgLength = msg.Length;
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.ASCII.GetBytes(msg));
        }
        public byte[] GetPacketByte() 
        {
            return _ms.ToArray() ;
        }
    }
}
