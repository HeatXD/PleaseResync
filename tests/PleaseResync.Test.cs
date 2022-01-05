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
        private const string LOCAL_ADDRESS = "127.0.0.1";

        [TestMethod]
        public void Test_VerifyDevices()
        {
            var session1 = new Peer2PeerSession(INPUT_SIZE, 3, 2);
            var session2 = new Peer2PeerSession(INPUT_SIZE, 3, 2);
            var session3 = new Peer2PeerSession(INPUT_SIZE, 3, 2);

            uint device1 = 0;
            uint device2 = 1;
            uint device3 = 2;

            session1.SetLocalDevice(device1, 1, 0, new UdpDeviceAdapter(LOCAL_PORT_1, null, 0));
            session2.SetLocalDevice(device2, 1, 0, new UdpDeviceAdapter(LOCAL_PORT_2, null, 0));
            session3.SetLocalDevice(device3, 1, 0, new UdpDeviceAdapter(LOCAL_PORT_3, null, 0));

            session1.AddRemoteDevice(device2, 1, new UdpDeviceAdapter(0, LOCAL_ADDRESS, LOCAL_PORT_2));
            session1.AddRemoteDevice(device3, 1, new UdpDeviceAdapter(0, LOCAL_ADDRESS, LOCAL_PORT_3));

            session2.AddRemoteDevice(device1, 1, new UdpDeviceAdapter(0, LOCAL_ADDRESS, LOCAL_PORT_1));
            session2.AddRemoteDevice(device3, 1, new UdpDeviceAdapter(0, LOCAL_ADDRESS, LOCAL_PORT_3));

            session3.AddRemoteDevice(device1, 1, new UdpDeviceAdapter(0, LOCAL_ADDRESS, LOCAL_PORT_1));
            session3.AddRemoteDevice(device2, 1, new UdpDeviceAdapter(0, LOCAL_ADDRESS, LOCAL_PORT_2));

            // Should roughly take ~5 iterations to get all sessions verified.
            for (int i = 0; i < 1; i++)
            {
                session1.DoPoll();
                session2.DoPoll();
                session3.DoPoll();
            }

            Assert.AreEqual(true, session1.IsRunning());
            Assert.AreEqual(true, session2.IsRunning());
            Assert.AreEqual(true, session3.IsRunning());
        }
    }
}
