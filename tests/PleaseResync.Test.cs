using PleaseResync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PleaseResyncTest
{
    [TestClass]
    public class PleaseResyncTest_Peer2PeerSession
    {
        private const uint INPUT_SIZE = 2;
        private const ushort LOCAL_PORT_1 = 7005;
        private const ushort LOCAL_PORT_2 = 7006;
        private const ushort LOCAL_PORT_3 = 7007;
        private const ushort LOCAL_PORT_4 = 7008;
        private const string LOCAL_ADDRESS = "127.0.0.1";

        [TestMethod]
        public void Test_VerifyDevices()
        {
            var session1 = new Peer2PeerSession(INPUT_SIZE, 4, 4);
            var session2 = new Peer2PeerSession(INPUT_SIZE, 4, 4);
            var session3 = new Peer2PeerSession(INPUT_SIZE, 4, 4);
            var session4 = new Peer2PeerSession(INPUT_SIZE, 4, 4);
            var sessions = new Session[] { session1, session2, session3, session4 };

            uint device1 = 0;
            uint device2 = 1;
            uint device3 = 2;
            uint device4 = 3;

            session1.SetLocalDevice(device1, 1, 0, new UdpDeviceAdapter(session1, LOCAL_PORT_1, null, 0));
            session1.AddRemoteDevice(device2, 1, new UdpDeviceAdapter(session1, 0, LOCAL_ADDRESS, LOCAL_PORT_2));
            session1.AddRemoteDevice(device3, 1, new UdpDeviceAdapter(session1, 0, LOCAL_ADDRESS, LOCAL_PORT_3));
            session1.AddRemoteDevice(device4, 1, new UdpDeviceAdapter(session1, 0, LOCAL_ADDRESS, LOCAL_PORT_4));

            session2.SetLocalDevice(device2, 1, 0, new UdpDeviceAdapter(session2, LOCAL_PORT_2, null, 0));
            session2.AddRemoteDevice(device1, 1, new UdpDeviceAdapter(session2, 0, LOCAL_ADDRESS, LOCAL_PORT_1));
            session2.AddRemoteDevice(device3, 1, new UdpDeviceAdapter(session2, 0, LOCAL_ADDRESS, LOCAL_PORT_3));
            session2.AddRemoteDevice(device4, 1, new UdpDeviceAdapter(session2, 0, LOCAL_ADDRESS, LOCAL_PORT_4));

            session3.SetLocalDevice(device3, 1, 0, new UdpDeviceAdapter(session3, LOCAL_PORT_3, null, 0));
            session3.AddRemoteDevice(device1, 1, new UdpDeviceAdapter(session3, 0, LOCAL_ADDRESS, LOCAL_PORT_1));
            session3.AddRemoteDevice(device2, 1, new UdpDeviceAdapter(session3, 0, LOCAL_ADDRESS, LOCAL_PORT_2));
            session3.AddRemoteDevice(device4, 1, new UdpDeviceAdapter(session3, 0, LOCAL_ADDRESS, LOCAL_PORT_4));

            session4.SetLocalDevice(device4, 1, 0, new UdpDeviceAdapter(session4, LOCAL_PORT_4, null, 0));
            session4.AddRemoteDevice(device1, 1, new UdpDeviceAdapter(session4, 0, LOCAL_ADDRESS, LOCAL_PORT_1));
            session4.AddRemoteDevice(device2, 1, new UdpDeviceAdapter(session4, 0, LOCAL_ADDRESS, LOCAL_PORT_2));
            session4.AddRemoteDevice(device3, 1, new UdpDeviceAdapter(session4, 0, LOCAL_ADDRESS, LOCAL_PORT_3));

            // Should roughly take ~5 iterations to get all sessions verified.
            for (int i = 0; i < 1; i++)
            {
                foreach (var session in sessions)
                {
                    session.DoPoll();
                }
            }

            foreach (var session in sessions)
            {
                Assert.AreEqual(true, session.IsRunning());
            }
        }
    }
}
