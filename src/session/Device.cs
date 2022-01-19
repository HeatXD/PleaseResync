using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace PleaseResync
{
    public class Device
    {
        #region Enum

        public enum DeviceType
        {
            Local,
            Remote,
            Spectator
        }

        public enum DeviceState
        {
            Syncing,
            Running,
            Disconnected,
        }

        #endregion

        #region Public

        public readonly uint Id;
        public readonly uint PlayerCount;
        public readonly DeviceType Type;

        public int RemoteFrame;
        public int RemoteFrameAdvantage;
        public uint LastAckedInputFrame;

        public DeviceState State;

        #endregion

        #region Private

        private const uint NUM_SYNC_ROUNDTRIPS = 5;
        private const uint SYNC_NEXT_RETRY_INTERVAL = 2000;
        private const uint SYNC_FIRST_RETRY_INTERVAL = 500;

        private readonly Session _session;

        private uint _lastSendTime;
        private uint _syncRoundtripsRemaining;
        private ushort _syncRoundtripsRandomRequest;
        private Queue<DeviceMessageQueueEntry> _sendQueue;

        #endregion

        public Device(Session session, uint deviceId, uint playerCount, DeviceType deviceType)
        {
            _session = session;
            _sendQueue = new Queue<DeviceMessageQueueEntry>();

            Id = deviceId;
            Type = deviceType;
            PlayerCount = playerCount;

            State = deviceType == DeviceType.Local ? DeviceState.Running : DeviceState.Syncing;
            RemoteFrame = 0;
            RemoteFrameAdvantage = 0;
        }

        public override string ToString()
        {
            return $"Device {new { Id, PlayerCount }}";
        }

        #region State Machine

        public void Sync()
        {
            uint now = Platform.GetCurrentTimeMS();

            if (Type == Device.DeviceType.Remote)
            {
                uint interval = _syncRoundtripsRemaining == NUM_SYNC_ROUNDTRIPS ? SYNC_FIRST_RETRY_INTERVAL : SYNC_NEXT_RETRY_INTERVAL;
                if (_lastSendTime + interval < now)
                {
                    _syncRoundtripsRandomRequest = Platform.GetRandomUnsignedShort();
                    SendMessage(new DeviceSyncMessage { DeviceId = Id, PlayerCount = PlayerCount, RandomRequest = _syncRoundtripsRandomRequest });
                }
            }
        }

        public void StartSyncing()
        {
            _syncRoundtripsRemaining = NUM_SYNC_ROUNDTRIPS;
            _syncRoundtripsRandomRequest = Platform.GetRandomUnsignedShort();
            SendMessage(new DeviceSyncMessage { DeviceId = Id, PlayerCount = PlayerCount, RandomRequest = _syncRoundtripsRandomRequest });
        }

        public void FinishedSyncing()
        {
            State = DeviceState.Running;
        }

        #endregion

        #region Sending and Receiving messages

        public void SendMessage(DeviceMessage message)
        {
            _lastSendTime = Platform.GetCurrentTimeMS();
            _sendQueue.Enqueue(new DeviceMessageQueueEntry { Time = _lastSendTime, Message = message });
            PumpSendQueue();
        }

        public void HandleMessage(DeviceMessage message)
        {
            switch (message)
            {
                case DeviceSyncMessage syncMessage:
                    Debug.Assert(syncMessage.PlayerCount == _session.AllDevices[syncMessage.DeviceId].PlayerCount);
                    SendMessage(new DeviceSyncConfirmMessage { DeviceId = Id, PlayerCount = PlayerCount, RandomResponse = syncMessage.RandomRequest });
                    break;
                case DeviceSyncConfirmMessage syncConfirmMessage:
                    Debug.Assert(syncConfirmMessage.PlayerCount == _session.AllDevices[syncConfirmMessage.DeviceId].PlayerCount);
                    if (syncConfirmMessage.RandomResponse == _syncRoundtripsRandomRequest)
                    {
                        _syncRoundtripsRemaining -= 1;
                        if (_syncRoundtripsRemaining > 0)
                        {
                            _syncRoundtripsRandomRequest = Platform.GetRandomUnsignedShort();
                            SendMessage(new DeviceSyncMessage { DeviceId = Id, PlayerCount = PlayerCount, RandomRequest = _syncRoundtripsRandomRequest });
                        }
                        else
                        {
                            FinishedSyncing();
                        }
                    }
                    break;
                case DeviceInputMessage inputMessage:
                    _session.AddRemoteInput(Id, inputMessage);
                    break;
                case DeviceInputAckMessage inputAckMessage:
                    UpdateAckedInputFrame(inputAckMessage);
                    break;
            }
        }

        private void UpdateAckedInputFrame(DeviceInputAckMessage inputAckMessage)
        {
            if (LastAckedInputFrame < inputAckMessage.Frame)
            {
                LastAckedInputFrame = inputAckMessage.Frame;
            }
        }

        private void PumpSendQueue()
        {
            while (_sendQueue.Count > 0)
            {
                _session.SendMessageTo(Id, _sendQueue.Dequeue().Message);
            }
        }

        #endregion
    }

    internal class DeviceMessageQueueEntry
    {
        public uint Time;
        public DeviceMessage Message;
    }
}
