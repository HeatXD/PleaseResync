using System.Linq;
using PleaseResync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PleaseResyncTest
{
    [TestClass]
    public class PleaseResyncTest_Peer2PeerSession
    {
        private const uint INPUT_SIZE = 2;
        private const ushort FRAME_DELAY = 0;
        private const ushort LOCAL_PORT_1 = 7001;
        private const ushort LOCAL_PORT_2 = 7002;
        private const ushort LOCAL_PORT_3 = 7003;
        private static readonly string LOCAL_ADDRESS = "127.0.0.1";

        [TestMethod]
        [TimeoutAttribute(5000)]
        public void Test_SyncDevices()
        {
            uint device1 = 0;
            uint device2 = 1;
            uint device3 = 2;

            var adapter1 = new UdpSessionAdapter(LOCAL_PORT_1);
            var adapter2 = new UdpSessionAdapter(LOCAL_PORT_2);
            var adapter3 = new UdpSessionAdapter(LOCAL_PORT_3);
            var adapters = new UdpSessionAdapter[] { adapter1, adapter2, adapter3 };

            var session1 = new Peer2PeerSession(INPUT_SIZE, 3, 3, adapter1);
            var session2 = new Peer2PeerSession(INPUT_SIZE, 3, 3, adapter2);
            var session3 = new Peer2PeerSession(INPUT_SIZE, 3, 3, adapter3);
            var sessions = new Peer2PeerSession[] { session1, session2, session3 };

            session1.SetLocalDevice(device1, 1, FRAME_DELAY);
            session1.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_2));
            session1.AddRemoteDevice(device3, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_3));

            session2.SetLocalDevice(device2, 1, FRAME_DELAY);
            session2.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_1));
            session2.AddRemoteDevice(device3, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_3));

            session3.SetLocalDevice(device3, 1, FRAME_DELAY);
            session3.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_1));
            session3.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_2));

            while (!sessions.All(session => session.IsRunning()))
            {
                foreach (var session in sessions)
                {
                    session.Poll();
                }

                System.Threading.Thread.Sleep(10);
            }

            foreach (var adapter in adapters)
            {
                adapter.Close();
            }
        }

        [TestMethod]
        [TimeoutAttribute(5000)]
        public void Test_SyncInputAcrossDevices()
        {
            uint device1 = 0;
            uint device2 = 1;

            var adapter1 = new UdpSessionAdapter(LOCAL_PORT_1);
            var adapter2 = new UdpSessionAdapter(LOCAL_PORT_2);
            var adapters = new UdpSessionAdapter[] { adapter1, adapter2 };

            var session1 = new Peer2PeerSession(INPUT_SIZE, 2, 2, adapter1);
            var session2 = new Peer2PeerSession(INPUT_SIZE, 2, 2, adapter2);
            var sessions = new Peer2PeerSession[] { session1, session2 };

            session1.SetLocalDevice(device1, 1, FRAME_DELAY);
            session1.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfiguration(LOCAL_ADDRESS, LOCAL_PORT_2));

            session2.SetLocalDevice(device2, 1, FRAME_DELAY);
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

            var actions1 = session1.AdvanceFrame(new byte[] { 1, 2 });
            var actions2 = session2.AdvanceFrame(new byte[] { 3, 4 });

            var advanceFrameAction1 = actions1.Find(action => action is SessionAdvanceFrameAction) as SessionAdvanceFrameAction;
            var advanceFrameAction2 = actions2.Find(action => action is SessionAdvanceFrameAction) as SessionAdvanceFrameAction;

            Assert.IsNotNull(advanceFrameAction1);
            Assert.IsNotNull(advanceFrameAction2);
            Assert.AreEqual((uint)1, advanceFrameAction1.Frame);
            Assert.AreEqual((uint)1, advanceFrameAction2.Frame);
            Assert.AreEqual((byte)1, advanceFrameAction1.Inputs[0]);
            Assert.AreEqual((byte)2, advanceFrameAction1.Inputs[1]);
            Assert.AreEqual((byte)0, advanceFrameAction1.Inputs[2]); // these inputs are not yet known to session1
            Assert.AreEqual((byte)0, advanceFrameAction1.Inputs[3]); // these inputs are not yet known to session1
            Assert.AreEqual((byte)0, advanceFrameAction2.Inputs[0]); // these inputs are not yet known to session2
            Assert.AreEqual((byte)0, advanceFrameAction2.Inputs[1]); // these inputs are not yet known to session2
            Assert.AreEqual((byte)3, advanceFrameAction2.Inputs[2]);
            Assert.AreEqual((byte)4, advanceFrameAction2.Inputs[3]);

            for (int i = 0; i < 20; i++)
            {
                foreach (var session in sessions)
                {
                    session.Poll(); // Give a chance to make the inputs flow from one session to another
                }

                actions1 = session1.AdvanceFrame(new byte[] { 1, 2 }); // send the same inputs as last frame
                actions2 = session2.AdvanceFrame(new byte[] { 3, 4 }); // send the same inputs as last frame
            }

            var advanceFrameActions1 = actions1.Where(action => action is SessionAdvanceFrameAction).ToArray();
            var advanceFrameActions2 = actions2.Where(action => action is SessionAdvanceFrameAction).ToArray();

            Assert.AreEqual(3, advanceFrameActions1.Length); // We should get a rollback for frame1 since we get different inputs, and a normal advance frame for frame2 (session1)
            Assert.AreEqual(3, advanceFrameActions2.Length); // We should get a rollback for frame1 since we get different inputs, and a normal advance frame for frame2 (session2)
            Assert.IsInstanceOfType(advanceFrameActions1[0], typeof(SessionAdvanceFrameAction));
            Assert.IsInstanceOfType(advanceFrameActions2[0], typeof(SessionAdvanceFrameAction));
            Assert.AreEqual((uint)1, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Frame); // First one should be the rollback for frame1 (session1)
            Assert.AreEqual((byte)1, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Inputs[0]);
            Assert.AreEqual((byte)2, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Inputs[1]);
            Assert.AreEqual((byte)3, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Inputs[2]);
            Assert.AreEqual((byte)4, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Inputs[3]);
            Assert.AreEqual((uint)1, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Frame); // First one should be the rollback for frame1 (session2)
            Assert.AreEqual((byte)1, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Inputs[0]);
            Assert.AreEqual((byte)2, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Inputs[1]);
            Assert.AreEqual((byte)3, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Inputs[2]);
            Assert.AreEqual((byte)4, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Inputs[3]);
            Assert.AreEqual((uint)2, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Frame); // Second one should be the normal advance for frame2 (session1)
            Assert.AreEqual((byte)1, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Inputs[0]);
            Assert.AreEqual((byte)2, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Inputs[1]);
            Assert.AreEqual((byte)3, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Inputs[2]);
            Assert.AreEqual((byte)4, ((SessionAdvanceFrameAction)advanceFrameActions1[0]).Inputs[3]);
            Assert.AreEqual((uint)2, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Frame); // Second one should be the normal advance for frame2 (session2)
            Assert.AreEqual((byte)1, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Inputs[0]);
            Assert.AreEqual((byte)2, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Inputs[1]);
            Assert.AreEqual((byte)3, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Inputs[2]);
            Assert.AreEqual((byte)4, ((SessionAdvanceFrameAction)advanceFrameActions2[0]).Inputs[3]);

            foreach (var adapter in adapters)
            {
                adapter.Close();
            }
        }
    }
}
