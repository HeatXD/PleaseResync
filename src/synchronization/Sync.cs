using System.Collections.Generic;

namespace PleaseResync
{
    public class Sync
    {
        private TimeSync _timeSync;

        public Sync()
        {
            _timeSync = new TimeSync();
        }

        // should be called after polling the remote devices
        public List<SessionAction> AdvanceSync()
        {
            UpdateSyncFrame();

            var actions = new List<SessionAction>();

            if (_timeSync.ShouldRollback())
            {
                actions.Add(new SessionLoadGameAction());
                for (int i = _timeSync.SyncFrame + 1; i <= _timeSync.LocalFrame; i++)
                {
                    actions.Add(new SessionAdvanceFrameAction());
                }
                actions.Add(new SessionSaveGameAction());
            }

            return actions;
        }

        public void UpdateSyncFrame()
        {
            int finalFrame = _timeSync.RemoteFrame;
            if (_timeSync.RemoteFrame > _timeSync.LocalFrame)
            {
                finalFrame = _timeSync.LocalFrame;
            }
            int foundFrame = finalFrame;
            for (int i = _timeSync.SyncFrame + 1; i <= finalFrame; i++)
            {
                // find the first frame where the predicted and remote inputs don't match
                // we assume the last frame is still correct
                // foundFrame =  i - 1;
                // break;
            }
            _timeSync.SyncFrame = foundFrame;
        }
    }
}
