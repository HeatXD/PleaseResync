namespace PleaseResync
{
    /// <summary>
    /// SessionAdapter is the interface used to implement a way for the Sessions to communicate with each other.
    /// </summary>
    public interface SessionAdapter
    {
        void Send(object message);
        object[] Receive();
    }
}
