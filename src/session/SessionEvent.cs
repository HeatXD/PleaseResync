namespace PleaseResync
{
    public abstract class SessionEvent
    {
        public abstract string Desc();
    }

    public class WaitSuggestionEvent : SessionEvent
    {
        public uint Frames;

        public override string Desc()
        {
            return "Wait Suggestion Event. Frames: " + Frames;
        }
    }

    public class DeviceSyncedEvent : SessionEvent
    {
        public uint DeviceId;

        public override string Desc()
        {
            return $"Remote Device {DeviceId} has been synced and ready to operate";
        }
    }
}
