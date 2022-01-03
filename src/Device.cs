namespace PleaseResync
{
    public enum DeviceType{
        LOCAL,
        REMOTE,
        SPECTATOR
    }
    public class Device
    {
        public int Id;
        public int RemoteFrame;
        public int RemoteFrameAdvantage;
        public int PlayerCount;
        public DeviceType Type;
        // todo Add IPeerNetAdaper
        public Device(int id, int playerCount, DeviceType deviceType)
        {
            Id = id;
            // no need for seperate functions anymore for local and remote devices
            Type = deviceType;
            PlayerCount = playerCount;
            RemoteFrame = 0;
            RemoteFrameAdvantage = 0;
        }
    }
}