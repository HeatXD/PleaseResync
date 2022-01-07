using System.Diagnostics;
using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// Peer2PeerSession implements a session for devices wanting to play your game together via network.
    /// </summary>
    public class Peer2PeerSession : Session
    {
        private Device _localDevice;
        private readonly Device[] _allDevices;
        private readonly SessionAdapter _sessionAdapter;

        public override Device LocalDevice => _localDevice;
        public override Device[] AllDevices => _allDevices;

        public Peer2PeerSession(uint inputSize, uint deviceCount, uint totalPlayerCount, SessionAdapter adapter) : base(inputSize, deviceCount, totalPlayerCount)
        {
            _allDevices = new Device[deviceCount];
            _sessionAdapter = adapter;
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            Debug.Assert(LocalDevice == null, $"Local device {deviceId} was already set.");
            Debug.Assert(_allDevices[deviceId] == null, $"Local device {deviceId} was already set.");

            _localDevice = new Device(this, deviceId, playerCount, Device.DeviceType.Local);
            _allDevices[deviceId] = LocalDevice;
        }
        public override void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration)
        {
            Debug.Assert(LocalDevice != null, "SetLocalDevice must be called before any call to AddRemoteDevice.");
            Debug.Assert(_allDevices[deviceId] == null, $"Remote device {deviceId} was already set.");

            _allDevices[deviceId] = new Device(this, deviceId, playerCount, Device.DeviceType.Remote);
            _sessionAdapter.AddRemote(deviceId, remoteConfiguration);
        }

        public override void Poll()
        {
            var messages = _sessionAdapter.ReceiveFrom();
            foreach (var (deviceId, message) in messages)
            {
                AllDevices[deviceId].HandleMessage(message);
            }
        }
        public override bool IsRunning()
        {
            throw new System.NotImplementedException();
        }

        public override void SetFrameInputs(byte[] input)
        {
            throw new System.NotImplementedException();
        }
        public override byte[] GetFrameInputs()
        {
            throw new System.NotImplementedException();
        }
        public override List<SessionAction> AdvanceFrame()
        {
            throw new System.NotImplementedException();
        }
    }
}
