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

        public override Device LocalDevice => _localDevice;
        public override Device[] AllDevices => _allDevices;

        public Peer2PeerSession(uint inputSize, uint deviceCount, uint totalPlayerCount) : base(inputSize, deviceCount, totalPlayerCount)
        {
            _allDevices = new Device[deviceCount];
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay, DeviceAdapter deviceAdapter)
        {
            Debug.Assert(LocalDevice == null, $"Local device {deviceId} was already set.");
            Debug.Assert(_allDevices[deviceId] == null, $"Local device {deviceId} was already set.");

            _localDevice = new Device(this, deviceId, playerCount, Device.DeviceType.Local, deviceAdapter);
            _allDevices[deviceId] = LocalDevice;
        }
        public override void AddRemoteDevice(uint deviceId, uint playerCount, DeviceAdapter deviceAdapter)
        {
            Debug.Assert(LocalDevice != null, "SetLocalDevice must be called before any call to AddRemoteDevice.");
            Debug.Assert(_allDevices[deviceId] == null, $"Remote device {deviceId} was already set.");

            _allDevices[deviceId] = new Device(this, deviceId, playerCount, Device.DeviceType.Remote, deviceAdapter);
        }

        public override void Poll()
        {
            foreach (var device in _allDevices)
            {
                device.Poll();
            }
        }
        public override bool IsRunning()
        {
            return true;
        }
        public override void HandleMessage(Device from, DeviceMessage message)
        {

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
