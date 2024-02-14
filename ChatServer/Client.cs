using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Client
    {
        public string BenutzerName { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;
        public Client(TcpClient client)
        { 
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte();
            BenutzerName = _packetReader.ReadMessage();

            Console.WriteLine($"({DateTime.Now}): Client wurde verbunden mit dem Benutzername; {BenutzerName} ");

            Task.Run(() => Prozess());
        } 

        void Prozess() 
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch(opcode)
                    {
                        case 5:
                            var sender = _packetReader.ReadSender();
                            var recipient = _packetReader.ReadRecipient();
                            var msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: message ist angekommen!{msg}, Send From {sender}, Send to: {recipient}");
                            Program.BroadcastMessage(sender, recipient, $"[{DateTime.Now}]:[{BenutzerName}]: {msg}");
                            break;
                        case 6:
                            var senderP = _packetReader.ReadSender();
                            var recipientP = _packetReader.ReadRecipient();
                            var msgP = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: message ist angekommen!{msgP}, Send From: {senderP}, Send to: {recipientP}");
                            Program.SendPrivateMessage(senderP, recipientP, $"[{DateTime.Now}]:[{BenutzerName}]: {msgP}");
                            break;
                        default:
                            break;
                    }
                }
                catch(Exception) 
                {
                    Console.WriteLine($"[{UID.ToString()}]: Disconnected!");
                    Program.BroadcastDisconnect(UID.ToString(), "Public");
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
