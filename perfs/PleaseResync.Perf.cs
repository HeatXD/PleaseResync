using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

using MessagePack;
using PleaseResync;

namespace PleaseResyncPerf
{
    public class MessageSerialization
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

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(new[]
            {
                BenchmarkConverter.TypeToBenchmarks(typeof(MessageSerialization))
            });
        }
    }
}
