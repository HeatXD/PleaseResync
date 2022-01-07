using System.Collections.Generic;

namespace PleaseResync
{
    /// <summary>
    /// DeviceAdapter is the interface used to implement a way for the Session to communicate with remote devices.
    /// </summary>
    public interface SessionAdapter
    {
        void SendTo(uint deviceId, DeviceMessage message);
        List<(uint deviceId, DeviceMessage message)> ReceiveFrom();

        void AddRemote(uint deviceId, object remoteConfiguration);
    }
}
