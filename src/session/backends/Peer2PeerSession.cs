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
        private readonly Device[] _everyDevices;

        public override Device LocalDevice => _localDevice;
        public override Device[] EveryDevices => _everyDevices;

        public Peer2PeerSession(uint inputSize, uint deviceCount, uint totalPlayerCount) : base(inputSize, deviceCount, totalPlayerCount)
        {
            _everyDevices = new Device[deviceCount];
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            Debug.Assert(LocalDevice == null, $"Local device {deviceId} was already set.");
            Debug.Assert(_everyDevices[deviceId] == null, $"Local device {deviceId} was already set.");

            _localDevice = new Device(this, deviceId, playerCount, Device.DeviceType.Local, null);
            _everyDevices[deviceId] = LocalDevice;
        }
        public override void AddRemoteDevice(uint deviceId, uint playerCount, DeviceAdapter deviceAdapter)
        {
            Debug.Assert(LocalDevice != null, "SetLocalDevice must be called before any call to AddRemoteDevice.");
            Debug.Assert(_everyDevices[deviceId] == null, $"Remote device {deviceId} was already set.");

            _everyDevices[deviceId] = new Device(this, deviceId, playerCount, Device.DeviceType.Remote, deviceAdapter);
        }

        public override void Poll()
        {
            foreach (var device in _everyDevices)
            {
                device.Poll();
            }
        }
        public override bool IsRunning()
        {
            return false; // TODO: check id all remote devices are verified
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
