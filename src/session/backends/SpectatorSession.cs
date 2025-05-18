using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PleaseResync.session.backends.utility;

namespace PleaseResync.session.backends
{
    public class SpectatorSession : Session
    {

        public SpectatorSession(uint inputSize, uint playerCount, SessionAdapter adapter, uint frameDelay = 30)
            : base(inputSize, 1, playerCount, false)
        {
            // we only need the remote device to create a spectator session.
            // the remote device is the broadcast device
            // which will provide all the needed inputs to proceed.
            _sessionAdapter = adapter;
            _broadcastStream = new BroadcastStream((int)frameDelay, inputSize * playerCount);
        }
        protected internal override Device LocalDevice => null;

        protected internal override Device[] AllDevices => throw new NotImplementedException();


        private int _currentFrame = 0;
        private Device _broadcastDevice = null;
        private BroadcastStream _broadcastStream = null;
        private readonly SessionAdapter _sessionAdapter;

        public override void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration)
        {
            Debug.Assert(_broadcastDevice == null, "Broadcast device was already set.");
            _sessionAdapter.AddRemote(deviceId, remoteConfiguration);
            _broadcastDevice = new Device(this, deviceId, playerCount, Device.DeviceType.Remote);
            _broadcastDevice.StartSyncing();
        }

        public override List<SessionAction> AdvanceFrame(byte[] localInput = null)
        {
            Debug.Assert(IsRunning(), "Session must be running before calling AdvanceFrame");
            Poll();

            var actions = new List<SessionAction>();
            if (_broadcastStream.GetFrameInput(out var frame, out var input))
            {
                _currentFrame = frame;
                actions.Add(new SessionAdvanceFrameAction(frame, input));

                if (frame == 1000|| frame == 5000 || frame == 10000)
                {
                    _broadcastStream.SaveToFile();
                }
            }
            return actions;
        }

        public override uint AverageRollbackFrames()
        {
            throw new NotImplementedException();
        }

        public override int Frame()
        {
            return _currentFrame;
        }

        public override int FrameAdvantage()
        {
            throw new NotImplementedException();
        }

        public override int FrameAdvantageDifference()
        {
            throw new NotImplementedException();
        }

        public override bool IsRunning()
        {
            return _broadcastDevice.State == Device.DeviceState.Running;
        }

        public override void Poll()
        {
            if (!IsRunning())
            {
                _broadcastDevice.Sync();
            }

            var messages = _sessionAdapter.ReceiveFrom();
            if (messages.Count == 0)
            {
                _broadcastDevice.TestConnection();
                return;
            }

            foreach (var (_, deviceId, message) in messages)
            {
                if (deviceId != _broadcastDevice.Id) continue; // discard messages from other devices
                _broadcastDevice.HandleMessage(message);
            }
        }

        public override int RemoteFrame()
        {
            throw new NotImplementedException();
        }

        public override int RemoteFrameAdvantage()
        {
            throw new NotImplementedException();
        }

        public override uint RollbackFrames()
        {
            throw new NotImplementedException();
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            throw new NotImplementedException();
        }

        public override int State()
        {
            throw new NotImplementedException();
        }

        protected internal override void AddRemoteInput(uint deviceId, DeviceInputMessage message)
        {
            if (deviceId != _broadcastDevice.Id) return; // discard messages from other devices
            var count = message.EndFrame - message.StartFrame + 1;
            var inpSize = (int)_broadcastStream.InputSize;

            for (var i = 0; i < count; i++)
            {
                var newFrame = (int)(i + message.StartFrame);
                _broadcastStream.AddFrameInput(newFrame, message.Input.Skip(i * inpSize).ToArray());
                SendMessageTo(_broadcastDevice.Id, new DeviceInputAckMessage { Frame = (uint)newFrame });
            }
        }

        protected internal override uint SendMessageTo(uint deviceId, DeviceMessage message)
        {
            if (deviceId != _broadcastDevice.Id) return 0; // dont send messages to other devices
            return _sessionAdapter.SendTo(deviceId, message);
        }

        public override void AddSpectatorDevice(object remoteConfiguration)
        {
            throw new NotImplementedException();
        }
    }
}
