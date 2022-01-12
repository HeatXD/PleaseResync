using System.Linq;
using PleaseResync;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessagePack;

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

        [MessagePackObject]
        public class TestState
        {
            [Key(0)]
            public uint frame;
            [Key(1)]
            public uint sum;

            public TestState(uint frame, uint sum)
            {
                this.frame = frame;
                this.sum = sum;
            }

            public void Update(byte[] playerInput)
            {
                frame++;

                foreach (var num in playerInput)
                {
                    sum += num;
                }
            }

            public override bool Equals(object obj)
            {
                return obj is TestState state &&
                       frame == state.frame &&
                       sum == state.sum;
            }

            public override int GetHashCode()
            {
                return System.HashCode.Combine(frame, sum);
            }
        }

        [TestMethod]
        [TimeoutAttribute(5000)]
        public void Test_RollbackSequence()
        {
            var sessionState1 = new TestState(0, 0);
            var sessionState2 = new TestState(0, 0);

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
            }

            for (int i = 0; i < 3; i++)
            {
                foreach (var session in sessions)
                {
                    session.Poll();
                }

                var sessionActions1 = session1.AdvanceFrame(new byte[] { 1, 2 });
                var sessionActions2 = session2.AdvanceFrame(new byte[] { 5, 6 });

                foreach (var action in sessionActions1)
                {
                    switch (action)
                    {
                        case SessionAdvanceFrameAction AFAction:
                            sessionState1.Update(AFAction.Inputs);
                            break;
                        case SessionLoadGameAction LGAction:
                            sessionState1 = MessagePackSerializer.Deserialize<TestState>(LGAction.Load());
                            break;
                        case SessionSaveGameAction SGAction:
                            SGAction.Save(MessagePackSerializer.Serialize(sessionState1));
                            break;
                    }
                }

                foreach (var action in sessionActions2)
                {
                    switch (action)
                    {
                        case SessionAdvanceFrameAction AFAction:
                            sessionState2.Update(AFAction.Inputs);
                            break;
                        case SessionLoadGameAction LGAction:
                            sessionState2 = MessagePackSerializer.Deserialize<TestState>(LGAction.Load());
                            break;
                        case SessionSaveGameAction SGAction:
                            SGAction.Save(MessagePackSerializer.Serialize(sessionState2));
                            break;
                    }
                }

                switch (i)
                {
                    case 0:
                        // games shouldnt be the same
                        Assert.AreNotEqual(sessionState1, sessionState2);
                        break;
                    case 1:
                        // games should be the same after rollback
                        Assert.AreEqual(sessionState1, sessionState2);
                        break;
                    case 2:
                        // games should still be the same after rollback
                        Assert.AreEqual(sessionState1, sessionState2);
                        break;
                }
            }

            foreach (var adapter in adapters)
            {
                adapter.Close();
            }
        }
    }
}
