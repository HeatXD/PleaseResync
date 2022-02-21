namespace PleaseResync
{
    public abstract class SessionEvent { };

    public class WaitSuggestionEvent : SessionEvent
    {
        public uint Frames;
    }
}
