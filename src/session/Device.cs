namespace PleaseResync
{
    public class Device
    {
        public enum DeviceType
        {
            LOCAL,
            REMOTE,
            SPECTATOR
        }
        public readonly int Id;
        public readonly uint PlayerCount;
        public readonly DeviceType Type;
        public int RemoteFrame;
        public int RemoteFrameAdvantage;
        public SessionAdapter Adapter;
        public Device(int id, uint playerCount, DeviceType deviceType)
        {
            Id = id;
            Type = deviceType;
            PlayerCount = playerCount;

            RemoteFrame = 0;
            RemoteFrameAdvantage = 0;
        }

        public Device(int id, uint playerCount, DeviceType deviceType, SessionAdapter adapter) : this(id, playerCount, deviceType)
        {
            Adapter = adapter;
        }
    }
}
