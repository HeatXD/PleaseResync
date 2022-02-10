using System.Linq;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

using MessagePack;
using PleaseResync;

namespace PleaseResyncPerf
{
    public class Benchmark_Session
    {
        private const uint INPUT_SIZE = 2;
        private const ushort FRAME_DELAY = 0;
        private const ushort LOCAL_PORT_1 = 7001;
        private const ushort LOCAL_PORT_2 = 7002;
        private const ushort LOCAL_PORT_3 = 7003;
        private const string LOCAL_ADDRESS = "127.0.0.1";

        [Benchmark]
        public void SyncSessions()
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

            for (var i = 0; i < 100000; i++)
            {
                session1.AdvanceFrame(new byte[] { 1, 2 });
                session2.AdvanceFrame(new byte[] { 3, 4 });

                foreach (var session in sessions)
                {
                    session.Poll();
                }
            }

            foreach (var adapter in adapters)
            {
                adapter.Close();
            }
        }
    }

    public class Benchmark_MessageSerialization
    {
        [Benchmark]
        public void CreateGameInputs()
        {
            for (var i = 0; i < 100000; i++)
            {
                new GameInput(0, 0, 1);
            }
        }

        [Benchmark]
        public void SerializeInputMessage()
        {
            var message = new DeviceInputMessage { StartFrame = 0, EndFrame = 0, Input = new byte[] { 1, 2, 3, 4 } };
            var messageSerialized = MessagePackSerializer.Serialize(message);
        }

        [Benchmark]
        public void SerializeAndDeserializeInputMessage()
        {
            var message = new DeviceInputMessage { StartFrame = 0, EndFrame = 0, Input = new byte[] { 1, 2, 3, 4 } };
            var messageSerialized = MessagePackSerializer.Serialize(message);
            var messageFromSerialization = MessagePackSerializer.Deserialize<DeviceInputMessage>(messageSerialized);
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(new[]
            {
                BenchmarkConverter.TypeToBenchmarks(typeof(Benchmark_Session)),
                BenchmarkConverter.TypeToBenchmarks(typeof(Benchmark_MessageSerialization)),
            });

            // Uncomment this and comment the lines above to run a perf test
            // var benchmarkSession = new Benchmark_Session();
            // benchmarkSession.SyncSessions();
        }
    }
}
