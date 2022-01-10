using System;
using System.Diagnostics;

namespace PleaseResync
{
    /// <summary>
    /// SessionAction is an action you must fulfill to give a chance to the Session to synchronize with other sessions.
    /// </summary>
    public abstract class SessionAction
    {
        /// <summary>
        /// Frame this action refers to.
        /// </summary>
        public int Frame;
    }

    /// <summary>
    /// SessionLoadGameAction is an action you must fulfill when the Session needs your game to rollback to a previous frame.
    /// </summary>
    public class SessionLoadGameAction : SessionAction
    {
        private StateStorage _storage;

        public SessionLoadGameAction(StateStorage storage, int frame)
        {
            Debug.Assert(frame >= 0);

            Frame = frame;
            _storage = storage;
        }

        public byte[] Load()
        {
            return _storage.LoadFrame(Frame);
        }

        public override string ToString() { return $"{typeof(SessionLoadGameAction)}: {new { Frame, _storage }}"; }
    }

    /// <summary>
    /// SessionSaveGameAction is an action you must fulfill when the Session needs to save your game state if it ever needs to rollback to that frame later.
    /// </summary>
    public class SessionSaveGameAction : SessionAction
    {
        private StateStorage _storage;

        public SessionSaveGameAction(StateStorage storage, int frame)
        {
            Debug.Assert(frame >= 0);

            Frame = frame;
            _storage = storage;
        }

        public void Save(byte[] gameState)
        {
            _storage.SaveToFrame(Frame, gameState);
        }

        public override string ToString() { return $"{typeof(SessionSaveGameAction)}: {new { Frame, _storage }}"; }
    }

    /// <summary>
    /// SessionAdvanceFrameAction is an action you must fulfill when the session needs the game to advance forward: either to perform a normal update or to resimulate an older frame.
    /// </summary>
    public class SessionAdvanceFrameAction : SessionAction
    {
        public byte[] Inputs;

        public SessionAdvanceFrameAction(byte[] inputs, int frame)
        {
            Debug.Assert(inputs != null);

            Frame = frame;
            Inputs = new byte[inputs.Length];
            Array.Copy(inputs, Inputs, inputs.Length);
        }

        public override string ToString() { return $"{typeof(SessionAdvanceFrameAction)}: {new { Frame, Inputs }}"; }
    }
}
