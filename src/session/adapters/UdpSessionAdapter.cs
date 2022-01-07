using System;
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
        private readonly IPEndPoint[] _remoteEndpoints;

        private IPEndPoint _remoteReceiveEndpoint;

        public UdpSessionAdapter(IPEndPoint endpoint)
        {
            _udpClient = new UdpClient();
            _udpClient.Client.Blocking = false;
            _udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(endpoint);
            _remoteEndpoints = new IPEndPoint[Session.LIMIT_DEVICE_COUNT];
            _remoteReceiveEndpoint = default(IPEndPoint);
        }

        public UdpSessionAdapter(ushort localPort) : this(new IPEndPoint(IPAddress.Any, localPort))
        {
        }

        public UdpSessionAdapter(string localAddress, ushort localPort) : this(new IPEndPoint(IPAddress.Parse(localAddress), localPort))
        {
        }

        public void SendTo(uint deviceId, DeviceMessage message)
        {
            var packet = MessagePackSerializer.Serialize(message);
            _udpClient.Send(packet, packet.Length, _remoteEndpoints[deviceId]);
        }

        public List<(uint deviceId, DeviceMessage message)> ReceiveFrom()
        {
            var messages = new List<(uint deviceId, DeviceMessage message)>();
            if (_udpClient.Available > 0)
            {
                var packet = _udpClient.Receive(ref _remoteReceiveEndpoint);
                var message = MessagePackSerializer.Deserialize<DeviceMessage>(packet);
                messages.Add((FindDeviceIdFromEndpoint(_remoteReceiveEndpoint), message));
            }
            return messages;
        }

        public void AddRemote(uint deviceId, object remoteConfiguration)
        {
            if (remoteConfiguration is IPEndPoint remoteEndpoint)
            {
                _remoteEndpoints[deviceId] = remoteEndpoint;
            }
            else
            {
                throw new Exception($"Remote configuration must be of type {typeof(IPEndPoint)}");
            }
        }

        private uint FindDeviceIdFromEndpoint(IPEndPoint endpoint)
        {
            for (uint deviceId = 0; deviceId < _remoteEndpoints.Length; deviceId++)
            {
                if (endpoint.Equals(_remoteEndpoints[deviceId]))
                {
                    return deviceId;
                }
            }
            throw new Exception($"Device ID not found for endpoint {endpoint}");
        }
    }
}