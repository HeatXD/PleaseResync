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
                    actions.Add(new SessionResimulateFrameAction());
                }
                
                actions.Add(new SessionSaveGameAction());
            }

            return actions;
        }
        public void UpdateSyncFrame()
        {
        }
    }
}
