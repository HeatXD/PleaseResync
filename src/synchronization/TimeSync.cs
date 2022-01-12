namespace PleaseResync
{
    internal class TimeSync
    {
        public const int InitialFrame = 0;
        public const int MaxRollbackFrames = 7;
        public const int FrameAdvantageLimit = 5;

        public int SyncFrame;
        public int LocalFrame;
        public int RemoteFrame;
        public int RemoteFrameAdvantage;

        public TimeSync()
        {
            SyncFrame = InitialFrame;
            LocalFrame = InitialFrame;
            RemoteFrame = InitialFrame;
            RemoteFrameAdvantage = 0;
        }

        public bool IsTimeSynced(Device[] devices)
        {
            int minRemoteFrame = int.MaxValue;
            int maxRemoteFrameAdvantage = int.MinValue;

            foreach (var device in devices)
            {
                if (device.Type == Device.DeviceType.Remote)
                {
                    // find min remote frame
                    if (device.RemoteFrame < minRemoteFrame)
                    {
                        minRemoteFrame = device.RemoteFrame;
                    }
                    // find max frame advantage
                    if (device.RemoteFrameAdvantage > maxRemoteFrameAdvantage)
                    {
                        maxRemoteFrameAdvantage = device.RemoteFrameAdvantage;
                    }
                }
            }
            // Set variables
            RemoteFrame = minRemoteFrame;
            RemoteFrameAdvantage = maxRemoteFrameAdvantage;
            // How far the client is ahead of the last reported frame by the remote clients           
            int localFrameAdvantage = LocalFrame - RemoteFrame;
            // How different is the frame advantage reported by the remote clients and this one
            int frameAdvantageDiff = localFrameAdvantage - RemoteFrameAdvantage;
            // Only allow the local client to get so far ahead of remote.
            return localFrameAdvantage < MaxRollbackFrames && frameAdvantageDiff <= FrameAdvantageLimit;
        }

        public bool ShouldRollback()
        {
            // No need to rollback if we don't have a frame after the previous sync frame to synchronize to.
            return LocalFrame > SyncFrame && RemoteFrame > SyncFrame;
        }
    }
}
