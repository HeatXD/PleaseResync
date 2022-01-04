using MessagePack;

namespace PleaseResync
{
    [Union(0, typeof(SessionSynchronizedMessage))]
    [Union(1, typeof(SessionSynchronizingMessage))]
    [MessagePackObject]
    public class SessionMessage { }

    [MessagePackObject]
    public class SessionSynchronizedMessage : SessionMessage { }

    [MessagePackObject]
    public class SessionSynchronizingMessage : SessionMessage { }
}
