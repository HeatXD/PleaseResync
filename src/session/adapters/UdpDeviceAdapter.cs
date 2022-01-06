using System.Net;
using MessagePack;
using System.Net.Sockets;
using System.Collections.Generic;

namespace PleaseResync
{
    public class UdpDeviceAdapter : DeviceAdapter
    {
        private const int SIO_UDP_CONNRESET = -1744830452;

        private readonly uint _deviceId;
        private readonly Session _session;
        private readonly UdpClient _udpClient;

        public UdpDeviceAdapter(Session session, uint deviceId, ushort localPort, string remoteAddress, ushort remotePort)
        {
            _session = session;
            _deviceId = deviceId;
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

        public List<(Device, DeviceMessage)> Receive()
        {
            var messages = new List<(Device, DeviceMessage)>();
            while (_udpClient.Available > 0)
            {
                var end = default(IPEndPoint);
                var packet = _udpClient.Receive(ref end);
                var device = FindDeviceByRemoteEndPoint(end);
                var message = MessagePackSerializer.Deserialize<DeviceMessage>(packet);
                messages.Add((device, message));
            }
            return messages;
        }

        private Device FindDeviceByRemoteEndPoint(IPEndPoint remoteEndPoint)
        {
            return _session.EveryDevices[_deviceId];
        }
    }
}
