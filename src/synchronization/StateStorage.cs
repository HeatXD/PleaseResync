using System.Diagnostics;

namespace PleaseResync
{
    internal class StateStorage
    {
        const int MAX_STORAGE_SIZE = 24;
        private readonly int _size;
        private readonly StateStorageEntry[] _states;

        public StateStorage(int maxRollbackFrames)
        {
            _size = maxRollbackFrames + MAX_STORAGE_SIZE;
            _states = new StateStorageEntry[_size];

            for (var i = 0; i < _size; i++)
            {
                _states[i] = new StateStorageEntry();
            }
        }

        public void SaveFrame(int frame, byte[] stateBuffer, uint stateChecksum = 0)
        {
            Debug.Assert(frame >= 0);
            Debug.Assert(stateBuffer != null, "Trying to save an empty state!");

            _states[frame % _size].Buffer = Platform.CloneByteArray(stateBuffer);
            _states[frame % _size].Checksum = stateChecksum;
            //Godot.GD.Print($"Saved state at frame {frame} with checksum {stateChecksum}");
        }

        public StateStorageEntry LoadFrame(int frame)
        {
            Debug.Assert(frame >= 0);
            Debug.Assert(_states[frame % _size] != null, "Trying to load an empty state!");

            return _states[frame % _size];
        }

        public bool CompareChecksums(int frame, uint checksum)
        {
            //Godot.GD.Print($"Checksum comparison: Frame:({frame} :: {_states[frame % _size].Checksum}), {checksum}");
            if (_states[frame % _size].Checksum == 0 || checksum == 0) return true;
            
            return _states[frame % _size].Checksum == checksum;
        }

        public uint GetChecksum(int frame)
        {
            return _states[frame % _size].Checksum;
        }
    }

    internal class StateStorageEntry
    {
        public byte[] Buffer;
        public uint Checksum;
    }
}
