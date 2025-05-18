using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;

namespace PleaseResync.session.backends.utility
{
    [MessagePackObject]
    public class ReplayFile
    {
        [Key(0)]
        public uint InputSize;
        [Key(1)]
        public int NumFrames;
        [Key(2)]
        public List<byte> InitialState;
        [Key(3)]
        public List<byte> InputFrames;

        public static void SaveToFile(uint inpSize, int numFrames, List<byte> initState, List<byte> inpFrames)
        {
            var file = new ReplayFile
            {
                InputSize = inpSize,
                NumFrames = numFrames,
                InitialState = Platform.RLEEncode(initState),
                InputFrames = Platform.RLEEncode(inpFrames)
            };

            var fileData = MessagePackSerializer.Serialize(file);
            string fileName = $"{Guid.NewGuid().ToString("N")}.PRReplay";

            File.WriteAllBytesAsync(fileName, fileData);
        }
    }
}
