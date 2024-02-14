using ChatServer.Net.IO;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;

namespace ChatServer
{
    class Program
    {
        static List<Client> _Benutzer;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _Benutzer = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 54888);
            _listener.Start();

            Console.WriteLine($"Server is listening on {_listener.LocalEndpoint}");

            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _Benutzer.Add(client);

                BroadcastConnection();
            }
        }
        static void BroadcastConnection()
        {
            foreach (var Benutzer in _Benutzer) 
            {
                foreach(var Bntz in _Benutzer)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMessage(Bntz.BenutzerName);
                    broadcastPacket.WriteMessage(Bntz.UID.ToString());
                    Benutzer.ClientSocket.Client.Send(broadcastPacket.GetPacketByte());
                }
            }
        }

        public static void BroadcastMessage(string sender, string recipient, string message) 
        {
            foreach(var Benutzer in _Benutzer )
            {
                var msgPaket = new PacketBuilder();
                msgPaket.WriteOpCode(5);
                msgPaket.WriteSender(sender);
                msgPaket.WriteRecipient(recipient);
                msgPaket.WriteMessage(message);
                Benutzer.ClientSocket.Client.Send(msgPaket.GetPacketByte());

            }
        }

        public static void BroadcastDisconnect(string uid , string sender)
        {
            var disconnectedUser = _Benutzer.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _Benutzer.Remove(disconnectedUser);
            
            foreach (var Benutzer in _Benutzer)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(69);
                broadcastPacket.WriteMessage(uid);
                Benutzer.ClientSocket.Client.Send(broadcastPacket.GetPacketByte());
            }
            BroadcastMessage(sender, "Public", $"[{disconnectedUser.BenutzerName}] Disconnected!");
        }

        public static void SendPrivateMessage(string sender, string recipientUsername, string message)
        {
            foreach (var benutzer in _Benutzer)
            {
                if (benutzer.BenutzerName == recipientUsername || benutzer.BenutzerName == sender)
                {
                    var msgPaket = new PacketBuilder();
                    msgPaket.WriteOpCode(5);
                    msgPaket.WriteSender(sender);
                    msgPaket.WriteRecipient(recipientUsername);
                    msgPaket.WriteMessage(message);
                    benutzer.ClientSocket.Client.Send(msgPaket.GetPacketByte());
                }
            }
        }
    }

}
