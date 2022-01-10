using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PleaseResync
{
    public class StateStorage
    {
        private List<byte[]> _states;
        private int _storageSize;
        public StateStorage(int maxRollbackFrames)
        {
            _storageSize = maxRollbackFrames + 2; // +2 for extra space for now. might be removed later
            _states = new List<byte[]>();
            for (int i = 0; i < _storageSize; i++)
            {
                _states.Add(new byte[1]);
            }
        }

        public byte[] LoadFrame(int frame)
        {
            Debug.Assert(frame >= 0);
            Debug.Assert(_states[frame % _storageSize] != null, "trying to load an empty state!");

            var tmpState = new byte[_states[frame % _storageSize].Length];
            Array.Copy(_states[frame % _storageSize], tmpState, _states[frame % _storageSize].Length);

            return tmpState;
        }

        public void SaveToFrame(int frame, byte[] gameState)
        {
            Debug.Assert(frame >= 0);
            Debug.Assert(gameState != null, "trying to save an empty gamestate!");

            _states[frame % _storageSize] = new byte[gameState.Length];
            Array.Copy(gameState, _states[frame % _storageSize], gameState.Length);
        }
    }
}
