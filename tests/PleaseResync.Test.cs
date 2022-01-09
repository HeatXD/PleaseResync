using System.Linq;
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
        [TimeoutAttribute(1000)]
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

            // Should roughly take 20 iterations to get all sessions verified.
            while (!sessions.All(session => session.IsRunning()))
            {
                System.Console.WriteLine("Iteration");
                foreach (var session in sessions)
                {
                    session.Poll();
                }

                System.Threading.Thread.Sleep(50);
            }
        }

        [TestMethod]
        [TimeoutAttribute(2000)]
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

            while (!sessions.All(session => session.IsRunning()))
            {
                foreach (var session in sessions)
                {
                    session.Poll();
                }

                System.Threading.Thread.Sleep(10);
            }

            foreach (var session in sessions)
            {
                session.Poll();
            }

            var actions1 = session1.AdvanceFrame(new byte[] { 2, 4 });
            var actions2 = session2.AdvanceFrame(new byte[] { 6, 7 });
        }
    }
}
