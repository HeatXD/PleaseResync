using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        [Key(4)]
        public List<byte> MetaData;

        public void Init(uint inputSize, List<byte> initialState, List<byte> metaData)
        {
            InputSize = inputSize;
            InitialState = new(initialState);
            MetaData = new(metaData);
        }

        public void SetData(int numFrames, List<byte> inputFrames)
        {
            NumFrames = numFrames;
            InputFrames = new(inputFrames);
        }

        public string Save(string folderPath = null)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                folderPath = "PRReplays";

            Directory.CreateDirectory(folderPath);

            var rawData = MessagePackSerializer.Serialize(this);

            var compressed = Platform.RLEEncode(rawData.ToList());

            string fileName = $"{Guid.NewGuid():N}.PRReplay";
            string fullPath = Path.Combine(folderPath, fileName);

            File.WriteAllBytes(fullPath, compressed.ToArray());
            return fullPath;
        }

        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Replay file not found.", filePath);

            var compressed = File.ReadAllBytes(filePath);

            var rawData = Platform.RLEDecode(compressed.ToList());

            var file = MessagePackSerializer.Deserialize<ReplayFile>(rawData.ToArray());

            Init(file.InputSize, file.InitialState, file.MetaData);
            SetData(file.NumFrames, file.InputFrames);
        }
    }
}
