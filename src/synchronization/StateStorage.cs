using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace PleaseResync
{
    internal class StateStorage
    {
        private readonly int _size;
        private readonly List<byte[]> _states;

        public StateStorage(int maxRollbackFrames)
        {
            _size = maxRollbackFrames + 2; // +2 for extra space for now. might be removed later
            _states = new List<byte[]>(_size);

            for (int i = 0; i < _size; i++)
            {
                _states.Add(new byte[1]);
            }
        }

        public byte[] LoadFrame(int frame)
        {
            Debug.Assert(frame >= 0);
            Debug.Assert(_states[frame % _size] != null, "trying to load an empty state!");

            var tmpState = new byte[_states[frame % _size].Length];
            Array.Copy(_states[frame % _size], tmpState, _states[frame % _size].Length);

            return tmpState;
        }

        public void SaveToFrame(int frame, byte[] gameState)
        {
            Debug.Assert(frame >= 0);
            Debug.Assert(gameState != null, "trying to save an empty gamestate!");

            _states[frame % _size] = new byte[gameState.Length];
            Array.Copy(gameState, _states[frame % _size], gameState.Length);
        }
    }
}
