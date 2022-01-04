using System.Diagnostics;
using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// Peer2PeerSession implements a session for devices wanting to play your game together via network.
    /// </summary>
    public class Peer2PeerSession : Session
    {
        private readonly Device[] Devices;

        public Peer2PeerSession(uint inputSize, uint deviceCount, uint totalPlayerCount) : base(inputSize, deviceCount, totalPlayerCount)
        {
            Devices = new Device[deviceCount];
        }

        public override void AddLocalDevice(int deviceId, uint playerCount, uint frameDelay)
        {
            Debug.Assert(Devices[deviceId] == null);

            Devices[deviceId] = new Device(deviceId, playerCount, Device.DeviceType.LOCAL);
        }
        public override void AddRemoteDevice(int deviceId, uint playerCount, SessionAdapter sessionAdapter)
        {
            Debug.Assert(Devices[deviceId] == null);

            Devices[deviceId] = new Device(deviceId, playerCount, Device.DeviceType.REMOTE, sessionAdapter);
        }

        public override void DoPoll()
        {
            throw new System.NotImplementedException();
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
