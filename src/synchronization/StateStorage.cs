using System;
using System.Diagnostics;

namespace PleaseResync
{
    public class StateStorage
    {
        private byte[] _state;

        public StateStorage() { }
        public byte[] Load()
        {
            Debug.Assert(_state != null, "trying to load an empty state!");

            byte[] tmpState = new byte[_state.Length];
            Array.Copy(_state, tmpState, _state.Length);

            return tmpState;
        }
        public void Save(byte[] gameState)
        {
            Debug.Assert(gameState != null, "trying to save an empty gamestate!");

            _state = new byte[gameState.Length];
            Array.Copy(gameState, _state, gameState.Length);
        }
    }
}
