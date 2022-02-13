using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System;

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
            Debug.Assert(localInput != null, "The session needs local input to advance the state");

            var actions = new List<SessionAction>();

            // save state of the initial frame
            if (_sync.CurrentFrame() == 0)
            {
                actions.Add(_sync.SaveCurrentFrame());
            }

            _sync.UpdateTimeSync();

            // the frame where the session has inputs form all devices
            int confirmedFrame = ConfirmedFrame();

            _sync.UpdateSyncFrame();

            // the frame where the inputs are still correct, 1 frame before the incorrect frame
            int syncFrame = _sync.SyncFrame();

            if (syncFrame != GameInput.NullFrame)
            {
                CorrectGameState(syncFrame, confirmedFrame, actions);
            }

            // TODO Send confirmed frames to spectators 
            // TODO Notify the local device whether to wait or not and for how long. to keep the simulation in sync

            //Add local input to session
            if (_sync.AddLocalInput(_localDevice.Id, localInput) != GameInput.NullFrame)
            {
                // send local device inputs to the remote devices
                _sync.SendLocalInputs(LocalDevice.Id);
            }

            // advance the game state
            var game = _sync.GetFrameInput(_sync.CurrentFrame());
            _sync.IncrementFrame();
            actions.Add(new SessionAdvanceFrameAction(_sync.CurrentFrame(), game.Inputs));

            return actions;
        }

        private void CorrectGameState(int syncFrame, int confirmedFrame, List<SessionAction> actions)
        {
            int currentFrame = _sync.CurrentFrame();

            actions.Add(_sync.LoadFrame(syncFrame));

            for (int i = syncFrame + 1; i <= currentFrame; i++)
            {
                var synced = _sync.GetFrameInput(i);

                _sync.IncrementFrame();
                actions.Add(new SessionAdvanceFrameAction(i, synced.Inputs));
                actions.Add(_sync.SaveCurrentFrame());
            }

            Debug.Assert(_sync.CurrentFrame() == currentFrame);
        }

        private int ConfirmedFrame()
        {
            int curr = _sync.CurrentFrame(), remote = _sync.RemoteFrame();
            return curr > remote ? remote : curr;
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
