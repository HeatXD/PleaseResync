using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// Peer2PeerSession implements a session for devices wanting to play your game together via network.
    /// </summary>
    public class Peer2PeerSession : Session
    {
        internal protected override Device LocalDevice => _localDevice;
        internal protected override Device[] AllDevices => _allDevices;

        private readonly Device[] _allDevices;
        private readonly SessionAdapter _sessionAdapter;

        private Sync _sync;
        private Device _localDevice;

        public Peer2PeerSession(uint inputSize, uint deviceCount, uint totalPlayerCount, SessionAdapter adapter) : base(inputSize, deviceCount, totalPlayerCount)
        {
            _allDevices = new Device[deviceCount];
            _sessionAdapter = adapter;
            _sync = new Sync(_allDevices, inputSize);
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            Debug.Assert(deviceId >= 0 && deviceId < DeviceCount, $"DeviceId {deviceId} should be between [0,  {DeviceCount}[");
            Debug.Assert(LocalDevice == null, $"Local device {deviceId} was already set.");
            Debug.Assert(_allDevices[deviceId] == null, $"Local device {deviceId} was already set.");

            _localDevice = new Device(this, deviceId, playerCount, Device.DeviceType.Local);
            _allDevices[deviceId] = LocalDevice;
            _sync.SetLocalDevice(deviceId, playerCount, frameDelay);
        }

        public override void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration)
        {
            Debug.Assert(deviceId >= 0 && deviceId < DeviceCount, $"DeviceId {deviceId} should be between [0,  {DeviceCount}[");
            Debug.Assert(LocalDevice != null, "SetLocalDevice must be called before any call to AddRemoteDevice.");
            Debug.Assert(_allDevices[deviceId] == null, $"Remote device {deviceId} was already set.");

            _sessionAdapter.AddRemote(deviceId, remoteConfiguration);
            _allDevices[deviceId] = new Device(this, deviceId, playerCount, Device.DeviceType.Remote);
            _allDevices[deviceId].StartSyncing();
            _sync.AddRemoteDevice(deviceId, playerCount);
        }

        public override void Poll()
        {
            Debug.Assert(_allDevices.All(device => device != null), "All devices must be Set/Added before calling Poll");

            if (!IsRunning())
            {
                foreach (var device in _allDevices)
                {
                    device.Sync();
                }
            }

            var messages = _sessionAdapter.ReceiveFrom();
            foreach (var (_, deviceId, message) in messages)
            {
                System.Console.WriteLine($"Received message from remote device {deviceId}: {message}");
                _allDevices[deviceId].HandleMessage(message);
            }
        }

        public override bool IsRunning()
        {
            return _allDevices.All(device => device.State == Device.DeviceState.Running);
        }

        public override List<SessionAction> AdvanceFrame(byte[] localInput)
        {
            Debug.Assert(IsRunning(), "Session must be running before calling AdvanceFrame");
            Debug.Assert(localInput != null);

            return _sync.AdvanceSync(_localDevice.Id, localInput);
        }

        internal protected override uint SendMessageTo(uint deviceId, DeviceMessage message)
        {
            System.Console.WriteLine($"Sending message to remote device {deviceId}: {message}");
            return _sessionAdapter.SendTo(deviceId, message);
        }

        internal protected override void AddRemoteInput(uint deviceId, DeviceInputMessage message)
        {

            uint inputCount = (message.EndFrame - message.StartFrame) + 1;
            uint inputSize = (uint)(message.Input.Length / inputCount);

            System.Console.WriteLine($"Recieved Inputs For Frames {message.StartFrame} to {message.EndFrame}. count: {inputCount}. size per input: {inputSize}");

            int inputIndex = 0;
            for (uint i = message.StartFrame; i <= message.EndFrame; i++)
            {
                byte[] inputsForFrame = new byte[message.Input.Length / inputCount];

                System.Array.Copy(message.Input, inputIndex * inputSize, inputsForFrame, 0, inputSize);
                _sync.AddRemoteInput(deviceId, (int)i, inputsForFrame);

                inputIndex++;
            }
        }
    }
}
