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
            Verifying,
            Verified,
            Disconnected,
        }

        #endregion

        #region Public

        public readonly uint Id;
        public readonly uint PlayerCount;
        public readonly DeviceType Type;

        public int RemoteFrame;
        public int RemoteFrameAdvantage;
        public DeviceState State;

        #endregion

        #region Private

        private readonly Session _session;

        #endregion

        public Device(Session session, uint deviceId, uint playerCount, DeviceType deviceType)
        {
            _session = session;

            Id = deviceId;
            Type = deviceType;
            PlayerCount = playerCount;

            State = DeviceState.Verifying;
            RemoteFrame = 0;
            RemoteFrameAdvantage = 0;
        }

        public override string ToString()
        {
            return $"Device {new { Id, PlayerCount }}";
        }

        #region State Machine

        private void Verify()
        {
            SendMessage(new DeviceVerifyMessage { DeviceId = Id, PlayerCount = PlayerCount });
        }

        #endregion

        #region Sending and Receiving messages

        internal void SendMessage(DeviceMessage message)
        {
        }

        internal void HandleMessage(DeviceMessage message)
        {
        }

        #endregion
    }
}
