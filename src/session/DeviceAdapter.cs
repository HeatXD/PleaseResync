using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// DeviceAdapter is the interface used to implement a way for the Session to communicate with other remote devices.
    /// </summary>
    public interface DeviceAdapter
    {
        void Send(DeviceMessage message);
        List<(Device, DeviceMessage)> Receive();
    }
}
