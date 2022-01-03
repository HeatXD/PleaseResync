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
    }

    /// <summary>
    /// SessionSaveGameAction is an action you must fulfill when the Session needs to save your game state if it ever needs to rollback to that frame later.
    /// </summary>
    public class SessionSaveGameAction : SessionAction
    {
    }

    /// <summary>
    /// SessionResimulateFrameAction is an action you must fulfill when the session is needs to re-simulate an older frame due to a rollback.
    /// </summary>
    public class SessionResimulateFrameAction : SessionAction
    {
        public byte[] Inputs;
    }
}
