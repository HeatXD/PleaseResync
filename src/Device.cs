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

        // todo Add IPeerNetAdaper

        public Device(int id, uint playerCount, DeviceType deviceType)
        {
            Id = id;
            Type = deviceType;
            PlayerCount = playerCount;

            RemoteFrame = 0;
            RemoteFrameAdvantage = 0;
        }
    }
}
