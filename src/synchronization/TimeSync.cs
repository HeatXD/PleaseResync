namespace PleaseResync
{
    internal class TimeSync
    {
        public const int MaxRollbackFrames = 6;

        public int SyncFrame;
        public int LocalFrame;
        public int RemoteFrame;
        public int RemoteFrameAdvantage;

        public TimeSync()
        {
            SyncFrame = GameInput.NullFrame;
            LocalFrame = GameInput.NullFrame;
            RemoteFrame = GameInput.NullFrame;
            RemoteFrameAdvantage = 0;
        }

        public bool PredictionLimitReached()
        {
            return LocalFrame >= MaxRollbackFrames && RemoteFrameAdvantage >= MaxRollbackFrames;
        }

        public void UpdateTimeSync(Device[] devices)
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
            RemoteFrame = minRemoteFrame;
            RemoteFrameAdvantage = maxRemoteFrameAdvantage;
        }
    }
}
