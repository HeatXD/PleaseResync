using System;
using System.Linq;
using System.Diagnostics;

namespace PleaseResync.input
{
    internal class GameInput
    {
        public const int NullFrame = -1;

        public readonly uint InputSize;
        public readonly uint PlayerCount;

        public int Frame;
        public byte[] Inputs;

        public GameInput(int frame, uint inputSize, uint playerCount)
        {
            Frame = frame;
            Inputs = new byte[inputSize * playerCount];
            InputSize = inputSize;
            PlayerCount = playerCount;
        }

        public GameInput(GameInput gameInput) : this(gameInput.Frame, gameInput.InputSize, gameInput.PlayerCount)
        {
            Array.Copy(gameInput.Inputs, Inputs, gameInput.Inputs.Length);
        }

        public void SetInputs(uint offset, uint playerCount, byte[] deviceInputs)
        {
            Debug.Assert(deviceInputs != null);
            Debug.Assert(offset + playerCount * InputSize <= Inputs.Length);
            Debug.Assert(deviceInputs.Length == playerCount * InputSize);

            Array.Copy(deviceInputs, 0, Inputs, offset * InputSize, deviceInputs.Length);
        }

        public bool Equal(GameInput other, bool inputsOnly)
        {
            if (!inputsOnly && Frame != other.Frame)
            {
                Platform.Log($"frames don't match: {Frame}, {other.Frame}");
            }
            if (InputSize != other.InputSize)
            {
                Platform.Log($"inputsize for a single player doesn't match: {InputSize}, {other.InputSize}");
            }
            if (Inputs.Length != other.Inputs.Length)
            {
                Platform.Log($"inputs array length don't match: {Inputs.Length}, {other.Inputs.Length}");
            }
            if (!Inputs.SequenceEqual(other.Inputs))
            {
                Platform.Log("inputs don't match");
            }
            Debug.Assert(Inputs.Length > 0 && other.Inputs.Length > 0);
            return (inputsOnly || Frame == other.Frame) &&
                   Inputs.Length == other.Inputs.Length &&
                   InputSize == other.InputSize &&
                   Inputs.SequenceEqual(other.Inputs);
        }
    }
}
