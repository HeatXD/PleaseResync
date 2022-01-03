using System.Collections.Generic;

namespace PleaseResync
{
    class TimeSync
    {
        public const int MaxRollbackFrames = 7;
        public const int FrameAdvantageLimit = 5;
        public const int InitialFrame = 0;
        public int LocalFrame;
        public int RemoteFrame;
        public int SyncFrame;
        public int RemoteFrameAdvantage;

        public TimeSync()
        {
            LocalFrame = InitialFrame;
            RemoteFrame = InitialFrame;
            SyncFrame = InitialFrame;

            RemoteFrameAdvantage = 0;
        }

        public bool IsTimeSynced(List<Device> devices)
        {
            int minRemoteFrame = InitialFrame - 1;
            int maxRemoteFrameAdvantage = 0;

            foreach (var device in devices)
            {
                if (device.Type == DeviceType.REMOTE)
                {
                    if (device.RemoteFrame < minRemoteFrame || device.RemoteFrame == InitialFrame - 1)
                    {
                        minRemoteFrame = device.RemoteFrame;
                    }

                    if (device.RemoteFrameAdvantage > maxRemoteFrameAdvantage)
                    {
                        maxRemoteFrameAdvantage = device.RemoteFrameAdvantage;
                    }
                }
            }

            // How far the client is ahead of the last reported frame by the remote clients           
            int localFrameAdvantage = LocalFrame - minRemoteFrame;
            // How different is the frame advantage reported by the remote clients and this one
            int frameAdvantageDiff = localFrameAdvantage - maxRemoteFrameAdvantage;
            return localFrameAdvantage < MaxRollbackFrames && frameAdvantageDiff <= FrameAdvantageLimit;
        }

        public bool ShouldRollback()
        {
            //No need to rollback if we don't have a frame after the previous sync frame to synchronize to.
            return LocalFrame > SyncFrame && RemoteFrame > SyncFrame;
        }
    }

}