using System.Net;
using MessagePack;
using System.Net.Sockets;
using System.Collections.Generic;

namespace PleaseResync
{
    public class UdpSessionAdapter : SessionAdapter
    {
        private const int SIO_UDP_CONNRESET = -1744830452;

        private readonly UdpClient _udpClient;
        private IPEndPoint _remoteEndPoint;

        public UdpSessionAdapter(int localPort, string remoteAddress, int remotePort)
        {
            _udpClient = new UdpClient(localPort);
            _udpClient.Connect(remoteAddress, remotePort);
            _udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), remotePort);
        }

        public void Send(SessionMessage message)
        {
            var packet = MessagePackSerializer.Serialize(message);
            _udpClient.Send(packet, packet.Length);
        }

        public List<SessionMessage> Receive()
        {
            var messages = new List<SessionMessage>();
            while (_udpClient.Available > 0)
            {
                var msg = _udpClient.Receive(ref _remoteEndPoint);
                messages.Add(MessagePackSerializer.Deserialize<SessionMessage>(msg));
            }
            return messages;
        }
    }
}
