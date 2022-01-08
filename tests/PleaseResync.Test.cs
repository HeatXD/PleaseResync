using System.Net;
using PleaseResync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PleaseResyncTest
{
    [TestClass]
    public class PleaseResyncTest_Peer2PeerSession
    {
        private const uint INPUT_SIZE = 2;
        private const ushort LOCAL_PORT_1 = 7001;
        private const ushort LOCAL_PORT_2 = 7002;
        private const ushort LOCAL_PORT_3 = 7003;
        private static readonly string LOCAL_ADDRESS = "127.0.0.1";

        [TestMethod]
        public void Test_SyncDevices()
        {
            uint device1 = 0;
            uint device2 = 1;
            uint device3 = 2;

            var session1 = new Peer2PeerSession(INPUT_SIZE, 3, 3, new UdpSessionAdapter(LOCAL_PORT_1));
            var session2 = new Peer2PeerSession(INPUT_SIZE, 3, 3, new UdpSessionAdapter(LOCAL_PORT_2));
            var session3 = new Peer2PeerSession(INPUT_SIZE, 3, 3, new UdpSessionAdapter(LOCAL_PORT_3));
            var sessions = new Session[] { session1, session2, session3 };

            session1.SetLocalDevice(device1, 1, 0);
            session1.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_2));
            session1.AddRemoteDevice(device3, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_3));

            session2.SetLocalDevice(device2, 1, 0);
            session2.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_1));
            session2.AddRemoteDevice(device3, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_3));

            session3.SetLocalDevice(device3, 1, 0);
            session3.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_1));
            session3.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_2));

            // Should roughly take ~500ms to get all sessions verified.
            for (int i = 0; i < 60; i++)
            {
                foreach (var session in sessions)
                {
                    session.Poll();
                }

                System.Threading.Thread.Sleep(100);
            }

            Assert.IsTrue(session1.IsRunning());
            Assert.IsTrue(session2.IsRunning());
            Assert.IsTrue(session3.IsRunning());

            for (int i = 0; i < 60; i++)
            {
                session1.AdvanceFrame(new byte[INPUT_SIZE]);

                System.Threading.Thread.Sleep(100);
            }
        }

        [TestMethod]
        public void Test_SyncInputAcrossDevices()
        {
            uint device1 = 0;
            uint device2 = 1;

            var session1 = new Peer2PeerSession(INPUT_SIZE, 2, 2, new UdpSessionAdapter(LOCAL_PORT_1));
            var session2 = new Peer2PeerSession(INPUT_SIZE, 2, 2, new UdpSessionAdapter(LOCAL_PORT_2));
            var sessions = new Session[] { session1, session2 };

            session1.SetLocalDevice(device1, 1, 0);
            session1.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_2));

            session2.SetLocalDevice(device2, 1, 0);
            session2.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_1));

            // Should roughly take ~500ms to get all sessions verified.
            for (int i = 0; i < 60; i++)
            {
                foreach (var session in sessions)
                {
                    session.Poll();
                }

                System.Threading.Thread.Sleep(100);
            }

            Assert.IsTrue(session1.IsRunning());
            Assert.IsTrue(session2.IsRunning());

            for (int i = 0; i < 60; i++)
            {
                var res = session1.AdvanceFrame(new byte[INPUT_SIZE]);

                System.Console.WriteLine(res);

                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
