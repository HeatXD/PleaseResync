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
        public uint Frame;
    }

    /// <summary>
    /// SessionLoadGameAction is an action you must fulfill when the Session needs your game to rollback to a previous frame.
    /// </summary>
    public class SessionLoadGameAction : SessionAction
    {
        public StateStorage Storage;

        public SessionLoadGameAction(StateStorage storage, int frame)
        {
            Frame = (uint)frame;
            Storage = storage;
        }

        public override string ToString() { return $"{typeof(SessionLoadGameAction)}: {new { Frame, Storage }}"; }
    }

    /// <summary>
    /// SessionSaveGameAction is an action you must fulfill when the Session needs to save your game state if it ever needs to rollback to that frame later.
    /// </summary>
    public class SessionSaveGameAction : SessionAction
    {
        public StateStorage Storage;

        public SessionSaveGameAction(StateStorage storage, int frame)
        {
            Frame = (uint)frame;
            Storage = storage;
        }

        public override string ToString() { return $"{typeof(SessionSaveGameAction)}: {new { Frame, Storage }}"; }
    }

    /// <summary>
    /// SessionAdvanceFrameAction is an action you must fulfill when the session needs the game to advance forward: either to perform a normal update or to resimulate an older frame.
    /// </summary>
    public class SessionAdvanceFrameAction : SessionAction
    {
        public byte[] Inputs;

        public SessionAdvanceFrameAction(byte[] inputs)
        {
            Debug.Assert(inputs != null);

            Inputs = new byte[inputs.Length];
            Array.Copy(inputs, Inputs, inputs.Length);
        }

        public override string ToString() { return $"{typeof(SessionAdvanceFrameAction)}: {new { Frame, Inputs }}"; }
    }
}
