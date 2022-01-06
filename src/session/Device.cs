namespace PleaseResync
{
    public class Device
    {
        #region Enums

        public enum DeviceType
        {
            Local,
            Remote,
            Spectator
        }

        public enum DeviceState
        {
            Verifying,
            Verified,
            Disconnected,
        }

        #endregion

        #region Publics

        public readonly uint Id;
        public readonly uint PlayerCount;
        public readonly DeviceType Type;
        public readonly DeviceAdapter Adapter;

        public int RemoteFrame;
        public int RemoteFrameAdvantage;
        public DeviceState State;

        #endregion

        #region Privates

        private readonly Session _session;

        private uint _lastSendTime;
        private uint _lastSequenceNumber;

        #endregion

        public Device(Session session, uint deviceId, uint playerCount, DeviceType deviceType, DeviceAdapter deviceAdapter)
        {
            _session = session;

            Id = deviceId;
            Type = deviceType;
            Adapter = deviceAdapter;
            PlayerCount = playerCount;

            State = DeviceState.Verifying;
            RemoteFrame = 0;
            RemoteFrameAdvantage = 0;
        }

        public override string ToString()
        {
            return $"Device {new { Id, PlayerCount }}";
        }

        #region Polling

        public void Poll()
        {
            uint now = Platform.GetCurrentTimeMS();
            uint nextInterval = 1000;

            if (Type == Device.DeviceType.Remote)
            {
                switch (State)
                {
                    case DeviceState.Verifying:
                        if (_lastSendTime + nextInterval < now)
                        {
                            Verify();
                        }
                        break;
                    case DeviceState.Verified:
                        // TODO: We are up and running
                        break;
                    case DeviceState.Disconnected:
                        // TODO: Tear down connection
                        break;
                }

                foreach (var (device, message) in Adapter.Receive())
                {
                    _session.HandleMessage(device, message);
                }
            }
        }

        #endregion

        #region State Machine

        private void Verify()
        {
            SendMessage(new DeviceVerifyMessage { DeviceId = Id, PlayerCount = PlayerCount });
        }

        #endregion

        #region Sending and Receiving messages

        private void SendMessage(DeviceMessage message)
        {
            _lastSendTime = Platform.GetCurrentTimeMS();
            message.SequenceNumber = _lastSequenceNumber++;
            Adapter.Send(message);
        }

        private void HandleMessage(DeviceMessage message)
        {

        }

        #endregion
    }
}
