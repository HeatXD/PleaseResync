using System.Diagnostics;
using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// Peer2PeerSession implements a session for devices wanting to play your game together via network.
    /// </summary>
    public class Peer2PeerSession : Session
    {
        private Device LocalDevice;
        private readonly Device[] Devices;

        public Peer2PeerSession(uint inputSize, uint deviceCount, uint totalPlayerCount) : base(inputSize, deviceCount, totalPlayerCount)
        {
            Devices = new Device[deviceCount];
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay, DeviceAdapter deviceAdapter)
        {
            Debug.Assert(LocalDevice == null, $"Local device {deviceId} was already set.");
            Debug.Assert(Devices[deviceId] == null, $"Local device {deviceId} was already set.");

            LocalDevice = new Device(deviceId, playerCount, Device.DeviceType.Local, deviceAdapter);
            Devices[deviceId] = LocalDevice;
        }
        public override void AddRemoteDevice(uint deviceId, uint playerCount, DeviceAdapter deviceAdapter)
        {
            Debug.Assert(LocalDevice != null, "SetLocalDevice must be called before any call to AddRemoteDevice.");
            Debug.Assert(Devices[deviceId] == null, $"Remote device {deviceId} was already set.");

            Devices[deviceId] = new Device(deviceId, playerCount, Device.DeviceType.Remote, deviceAdapter);
            Devices[deviceId].Verify();
        }

        public override void DoPoll()
        {
            foreach (var device in Devices)
            {
                device.DoPoll();
            }
        }
        public override bool IsRunning()
        {
            return true;
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
