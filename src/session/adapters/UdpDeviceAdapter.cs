using System.Net;
using MessagePack;
using System.Net.Sockets;
using System.Collections.Generic;

namespace PleaseResync
{
    public class UdpDeviceAdapter : DeviceAdapter
    {
        private const int SIO_UDP_CONNRESET = -1744830452;

        private readonly UdpClient _udpClient;

        private IPEndPoint _remoteEndPoint;

        public UdpDeviceAdapter(ushort localPort, string remoteAddress, ushort remotePort)
        {
            _udpClient = localPort == 0 ? new UdpClient() : new UdpClient(localPort);
            if (remoteAddress != null && remotePort > 0)
            {
                _udpClient.Connect(remoteAddress, remotePort);
                _udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
                _remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), remotePort);
            }
        }

        public void Send(DeviceMessage message)
        {
            var packet = MessagePackSerializer.Serialize(message);
            System.Console.WriteLine($"Sent {message}");
            _udpClient.Send(packet, packet.Length);
        }

        public List<DeviceMessage> Receive()
        {
            var messages = new List<DeviceMessage>();
            while (_udpClient.Available > 0)
            {
                var packet = _udpClient.Receive(ref _remoteEndPoint);
                var message = MessagePackSerializer.Deserialize<DeviceMessage>(packet);
                System.Console.WriteLine($"Received {message}");
                messages.Add(message);
            }
            return messages;
        }
    }
}
