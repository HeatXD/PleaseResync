using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// DeviceAdapter is the interface used to implement a way for the devices to communicate with each other.
    /// </summary>
    public interface DeviceAdapter
    {
        void Send(DeviceMessage message);
        List<DeviceMessage> Receive();
    }
}
