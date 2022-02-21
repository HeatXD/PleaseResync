namespace PleaseResync
{
    public abstract class SessionEvent
    {
        public virtual string Desc() => "Base Session Event Class";
    };

    public class WaitSuggestionEvent : SessionEvent
    {
        public uint Frames;

        public override string Desc()
        {
            return "Wait Suggestion Event. Frames: " + Frames;
        }

    }
}
