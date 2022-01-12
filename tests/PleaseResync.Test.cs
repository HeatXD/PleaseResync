using System.Linq;
using MessagePack;
using PleaseResync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PleaseResyncTest
{
    [TestClass]
    public class PleaseResyncTest_Peer2PeerSession
    {
        private const uint ITERATIONS = 3;
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
            session1.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_2));
            session1.AddRemoteDevice(device3, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_3));

            session2.SetLocalDevice(device2, 1, FRAME_DELAY);
            session2.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_1));
            session2.AddRemoteDevice(device3, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_3));

            session3.SetLocalDevice(device3, 1, FRAME_DELAY);
            session3.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_1));
            session3.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_2));

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
            session1.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_2));

            session2.SetLocalDevice(device2, 1, FRAME_DELAY);
            session2.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_1));

            while (!sessions.All(session => session.IsRunning()))
            {
                foreach (var session in sessions)
                {
                    session.Poll();
                }
            }

            for (int i = 0; i < 10; i++)
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
            session1.AddRemoteDevice(device2, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_2));

            session2.SetLocalDevice(device2, 1, FRAME_DELAY);
            session2.AddRemoteDevice(device1, 1, UdpSessionAdapter.CreateRemoteConfig(LOCAL_ADDRESS, LOCAL_PORT_1));

            while (!sessions.All(session => session.IsRunning()))
            {
                foreach (var session in sessions)
                {
                    session.Poll();
                }
            }

            // step one: advance to the first frame
            // since it's the first frame: there is no chance to have a rollback right now
            var actions1 = session1.AdvanceFrame(new byte[] { 1, 2 });
            var actions2 = session2.AdvanceFrame(new byte[] { 3, 4 });

            // session1
            Assert.IsInstanceOfType(actions1[0], typeof(SessionSaveGameAction)); // the first ever action must be to save the state before any simulation, otherwise we would never be able to rollback before the first frame
            Assert.AreEqual(0, ((SessionSaveGameAction)actions1[0]).Frame);
            Assert.IsInstanceOfType(actions1[1], typeof(SessionAdvanceFrameAction)); // then we are ready to simulate the first frame
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions1[1]).Frame);
            Assert.AreEqual((byte)1, ((SessionAdvanceFrameAction)actions1[1]).Inputs[0]); // local input
            Assert.AreEqual((byte)2, ((SessionAdvanceFrameAction)actions1[1]).Inputs[1]); // local input
            Assert.AreEqual((byte)0, ((SessionAdvanceFrameAction)actions1[1]).Inputs[2]); // these inputs are not yet known to session1 and should be zero
            Assert.AreEqual((byte)0, ((SessionAdvanceFrameAction)actions1[1]).Inputs[3]); // these inputs are not yet known to session1 and should be zero
            Assert.IsInstanceOfType(actions1[2], typeof(SessionSaveGameAction)); // then we save the simulation at frame 1, this simulation maybe incorrect, but we will know later when remote inputs arrive
            Assert.AreEqual(1, ((SessionSaveGameAction)actions1[2]).Frame);
            // session2
            Assert.IsInstanceOfType(actions2[0], typeof(SessionSaveGameAction)); // the first ever action must be to save the state before any simulation, otherwise we would never be able to rollback before the first frame
            Assert.AreEqual(0, ((SessionSaveGameAction)actions2[0]).Frame);
            Assert.IsInstanceOfType(actions2[1], typeof(SessionAdvanceFrameAction)); // then we are ready to simulate the first frame
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions2[1]).Frame);
            Assert.AreEqual((byte)0, ((SessionAdvanceFrameAction)actions2[1]).Inputs[0]); // these inputs are not yet known to session1 and should be zero
            Assert.AreEqual((byte)0, ((SessionAdvanceFrameAction)actions2[1]).Inputs[1]); // these inputs are not yet known to session1 and should be zero
            Assert.AreEqual((byte)3, ((SessionAdvanceFrameAction)actions2[1]).Inputs[2]); // local input
            Assert.AreEqual((byte)4, ((SessionAdvanceFrameAction)actions2[1]).Inputs[3]); // local input
            Assert.IsInstanceOfType(actions2[2], typeof(SessionSaveGameAction)); // then we save the simulation at frame 1, this simulation maybe incorrect, but we will know later when remote inputs arrive
            Assert.AreEqual(1, ((SessionSaveGameAction)actions1[2]).Frame);

            // give a chance to remote inputs to flow from one session to another
            for (var i = 0; i < ITERATIONS; i++)
            {
                foreach (var session in sessions)
                {
                    session.Poll();

                }
                System.Threading.Thread.Sleep(100);
            }

            // step two: advance to the second frame
            // since remote inputs should be arrived now, the simulation for the first frame for both sessions were done with incomplete inputs...
            // ...so we have to rollback and replay the first frame with the newly arrived inputs
            actions1 = session1.AdvanceFrame(new byte[] { 1, 2 });
            actions2 = session2.AdvanceFrame(new byte[] { 3, 4 });

            // session1
            Assert.IsInstanceOfType(actions1[0], typeof(SessionLoadGameAction)); // rollback before the first frame
            Assert.AreEqual(0, ((SessionLoadGameAction)actions1[0]).Frame);
            Assert.IsInstanceOfType(actions1[1], typeof(SessionAdvanceFrameAction)); // resimulate frame 1
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions1[1]).Frame);
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions1[1]).Inputs[0]); // local input
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions1[1]).Inputs[1]); // local input
            Assert.AreEqual(3, ((SessionAdvanceFrameAction)actions1[1]).Inputs[2]); // we got the remote inputs now :)
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions1[1]).Inputs[3]); // we got the remote inputs now :)
            Assert.IsInstanceOfType(actions1[2], typeof(SessionSaveGameAction)); // save state at frame 1, since we know this state is the last correct state
            Assert.IsInstanceOfType(actions1[3], typeof(SessionAdvanceFrameAction)); // simulate frame 2
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions1[3]).Frame);
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions1[3]).Inputs[0]); // local input
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions1[3]).Inputs[1]); // local input
            Assert.AreEqual(3, ((SessionAdvanceFrameAction)actions1[3]).Inputs[2]); // we predict the input for the remote device to be the same as the previous frame
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions1[3]).Inputs[3]); // we predict the input for the remote device to be the same as the previous frame
            Assert.IsInstanceOfType(actions1[4], typeof(SessionSaveGameAction)); // then we save the simulation at frame 2, this simulation maybe incorrect, but we will know later when remote inputs arrive
            Assert.AreEqual(2, ((SessionSaveGameAction)actions1[4]).Frame);
            // session2
            Assert.IsInstanceOfType(actions2[0], typeof(SessionLoadGameAction)); // rollback before the first frame
            Assert.AreEqual(0, ((SessionLoadGameAction)actions2[0]).Frame);
            Assert.IsInstanceOfType(actions2[1], typeof(SessionAdvanceFrameAction)); // resimulate frame 1
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions2[1]).Frame);
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions2[1]).Inputs[0]); // we got the remote inputs now :)
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions2[1]).Inputs[1]); // we got the remote inputs now :)
            Assert.AreEqual(3, ((SessionAdvanceFrameAction)actions2[1]).Inputs[2]); // local input
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions2[1]).Inputs[3]); // local input
            Assert.IsInstanceOfType(actions2[2], typeof(SessionSaveGameAction)); // save state at frame 1, since we know this state is the last correct state
            Assert.IsInstanceOfType(actions2[3], typeof(SessionAdvanceFrameAction)); // simulate frame 2
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions2[3]).Frame);
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions2[3]).Inputs[0]); // we predict the input for the remote device to be the same as the previous frame
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions2[3]).Inputs[1]); // we predict the input for the remote device to be the same as the previous frame
            Assert.AreEqual(3, ((SessionAdvanceFrameAction)actions2[3]).Inputs[2]); // local input
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions2[3]).Inputs[3]); // local input
            Assert.IsInstanceOfType(actions2[4], typeof(SessionSaveGameAction)); // then we save the simulation at frame 2, this simulation maybe incorrect, but we will know later when remote inputs arrive
            Assert.AreEqual(2, ((SessionSaveGameAction)actions2[4]).Frame);

            // give a chance to remote inputs to flow from one session to another
            for (var i = 0; i < ITERATIONS; i++)
            {
                foreach (var session in sessions)
                {
                    session.Poll();

                }
                System.Threading.Thread.Sleep(100);
            }

            // step three: advance to the third frame
            // we are sending the same inputs as frame 2 and there should be no rollbacks since the same inputs should be predicted
            actions1 = session1.AdvanceFrame(new byte[] { 1, 2 });
            actions2 = session2.AdvanceFrame(new byte[] { 3, 4 });

            // session1
            Assert.IsInstanceOfType(actions1[0], typeof(SessionAdvanceFrameAction));
            Assert.AreEqual(3, ((SessionSaveGameAction)actions1[1]).Frame);
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions1[0]).Inputs[0]); // local input
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions1[0]).Inputs[1]); // local input
            Assert.AreEqual(3, ((SessionAdvanceFrameAction)actions1[0]).Inputs[2]); // predicted input
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions1[0]).Inputs[3]); // predicted input
            // session2
            Assert.IsInstanceOfType(actions2[0], typeof(SessionAdvanceFrameAction));
            Assert.AreEqual(3, ((SessionSaveGameAction)actions2[1]).Frame);
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions2[0]).Inputs[0]); // predicted input
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions2[0]).Inputs[1]); // predicted input
            Assert.AreEqual(3, ((SessionAdvanceFrameAction)actions2[0]).Inputs[2]); // local input
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions2[0]).Inputs[3]); // local input

            // step four: advance to the fourth frame
            // this time we will send different inputs from session 2 to force a one-sided rollback on session 1
            actions1 = session1.AdvanceFrame(new byte[] { 1, 2 }); // keep same inputs
            actions2 = session2.AdvanceFrame(new byte[] { 5, 6 }); // different inputs

            // session1
            Assert.IsInstanceOfType(actions1[0], typeof(SessionAdvanceFrameAction));
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions1[0]).Frame);
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions1[0]).Inputs[0]); // local input
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions1[0]).Inputs[1]); // local input
            Assert.AreEqual(3, ((SessionAdvanceFrameAction)actions1[0]).Inputs[2]); // predicted input (but wrong), should be rolled back on the next frame
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions1[0]).Inputs[3]); // predicted input (but wrong), should be rolled back on the next frame
            Assert.IsInstanceOfType(actions1[1], typeof(SessionSaveGameAction)); // then we save the simulation at frame 4, this simulation maybe incorrect, but we will know later when remote inputs arrive
            Assert.AreEqual(4, ((SessionSaveGameAction)actions1[1]).Frame);
            // session2
            Assert.IsInstanceOfType(actions2[0], typeof(SessionAdvanceFrameAction));
            Assert.AreEqual(4, ((SessionAdvanceFrameAction)actions2[0]).Frame);
            Assert.AreEqual(1, ((SessionAdvanceFrameAction)actions2[0]).Inputs[0]); // predicted input (and right), should NOT be rolled back on the next frame
            Assert.AreEqual(2, ((SessionAdvanceFrameAction)actions2[0]).Inputs[1]); // predicted input (and right), should NOT be rolled back on the next frame
            Assert.AreEqual(5, ((SessionAdvanceFrameAction)actions2[0]).Inputs[2]); // local input
            Assert.AreEqual(6, ((SessionAdvanceFrameAction)actions2[0]).Inputs[3]); // local input
            Assert.IsInstanceOfType(actions2[1], typeof(SessionSaveGameAction)); // then we save the simulation at frame 4, this simulation maybe incorrect, but we will know later when remote inputs arrive
            Assert.AreEqual(4, ((SessionSaveGameAction)actions2[1]).Frame);

            // step five: advance to the fifth frame
            // we don't really care about the inputs we send since this is the last test...
            // ...but we make sure that the one-sided rollback for the fourth frame is correctly applied
            actions1 = session1.AdvanceFrame(new byte[] { 1, 2 });
            actions2 = session2.AdvanceFrame(new byte[] { 5, 6 });

            // TODO: checks

            foreach (var adapter in adapters)
            {
                adapter.Close();
            }
        }
    }
}
