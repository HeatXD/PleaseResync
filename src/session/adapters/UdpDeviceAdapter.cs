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

        public UdpDeviceAdapter(ushort localPort, string remoteAddress, ushort remotePort)
        {

            _udpClient = new UdpClient();
            _udpClient.Client.Blocking = false;
            _udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, localPort));
            _udpClient.Connect(remoteAddress, remotePort);
        }

        public void Send(DeviceMessage message)
        {
            var packet = MessagePackSerializer.Serialize(message);
            _udpClient.Send(packet, packet.Length);
        }

        public List<DeviceMessage> Receive()
        {
            var messages = new List<DeviceMessage>();
            while (_udpClient.Available > 0)
            {
                var end = default(IPEndPoint);
                var packet = _udpClient.Receive(ref end);
                var message = MessagePackSerializer.Deserialize<DeviceMessage>(packet);
                messages.Add(message);
            }
            return messages;
        }
    }
}
